using Binner.Common.IO;
using Binner.Common.Services.Authentication;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Authentication;
using Binner.Model.Configuration;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly RequestContextAccessor _requestContext;
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly JwtService _jwt;
        private readonly HttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(ILogger<AuthenticationService> logger, IStorageProvider storageProvider, IDbContextFactory<BinnerContext> contextFactory, RequestContextAccessor requestContextAccessor, JwtService jwt, WebHostServiceConfiguration configuration, HttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _contextFactory = contextFactory;
            _jwt = jwt;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var user = await context.Users
                    .Where(x => x.EmailAddress == model.Username)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    var userContext = Map(user);
                    var isLoginAllowed = false;
                    if (string.IsNullOrEmpty(user.PasswordHash) && string.IsNullOrEmpty(model.Password))
                    {
                        // special case for lost passwords on local installations only, requires database-level password clear as it can't be done via the api
                        isLoginAllowed = true;
                    }
                    else
                    {
                        // validate password
                        isLoginAllowed = PasswordHasher.Verify(model.Password, user.PasswordHash);
                    }
                    if (isLoginAllowed)
                    {
                        // authenticated
                        var authenticationResponse = await CreateAuthenticationLoginAsync(context, user, userContext);
                        if (!authenticationResponse.IsAuthenticated)
                            _logger.LogWarning($"[{nameof(AuthenticateAsync)}] {authenticationResponse.Message}. Username: '{model.Username}' IP: {_requestContext.GetIpAddress()}");

                        context.UserLoginHistory.Add(new Data.Model.UserLoginHistory
                        {
                            IsSuccessful = authenticationResponse.IsAuthenticated,
                            CanLogin = authenticationResponse.CanLogin,
                            Message = authenticationResponse.Message,
                            Ip = _requestContext.GetIp(),
                            User = user,
                        });
                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return authenticationResponse;
                    }
                }

                context.UserLoginHistory.Add(new Data.Model.UserLoginHistory
                {
                    EmailAddress = model.Username,
                    IsSuccessful = false,
                    CanLogin = false,
                    Ip = _requestContext.GetIp(),
                    User = user,
                });
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogWarning($"[{nameof(AuthenticateAsync)}] Invalid username or password! Username: {model.Username} IP: {_requestContext.GetIpAddress()}");
                return new AuthenticationResponse(false, "Invalid username or password.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[{nameof(AuthenticateAsync)}] Unhandled exception '{ex.Message}'! Username: {model.Username} IP: {_requestContext.GetIpAddress()}");
                transaction.Rollback();
                throw;
            }
        }

        private async Task<AuthenticationResponse> CreateAuthenticationLoginAsync(BinnerContext context, Data.Model.User user, UserContext userContext)
        {
            // are they allowed to login?
            if (!user.IsEmailConfirmed)
                return new AuthenticationResponse(false, "You must confirm your email address before logging in.");
            if (user.DateLockedUtc != null)
                return new AuthenticationResponse(false, "Account is locked. Please contact your admin.");

            userContext.CanLogin = true;

            user.DateLastLoginUtc = DateTime.UtcNow;
            user.DateLastActiveUtc = DateTime.UtcNow;

            var authenticatedTokens = await GetAuthenticatedTokensAsync(context, userContext);
            context.UserTokens.Add(new Data.Model.UserToken
            {
                Token = authenticatedTokens.RefreshToken,
                DateCreatedUtc = authenticatedTokens.DateCreated,
                DateExpiredUtc = authenticatedTokens.DateExpires,
                TokenTypeId = TokenTypes.RefreshToken,
                UserId = user.UserId,
                OrganizationId = user.OrganizationId,
                Ip = _requestContext.GetIp()
            });
            context.UserTokens.Add(new Data.Model.UserToken
            {
                Token = authenticatedTokens.ImagesToken,
                DateCreatedUtc = authenticatedTokens.DateCreated,
                DateExpiredUtc = authenticatedTokens.DateExpires,
                TokenTypeId = TokenTypes.ImagesToken,
                UserId = user.UserId,
                OrganizationId = user.OrganizationId,
                Ip = _requestContext.GetIp()
            });
            await context.SaveChangesAsync();

            // remove old refresh tokens from user
            await RemoveOldUserTokensAsync(context, user);
            return new AuthenticationResponse(userContext, authenticatedTokens);
        }

        public async Task<AuthenticationResponse> RefreshTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            using var context = await _contextFactory.CreateDbContextAsync();
            // todo: seems to be causing deadlocks, need to investigate
            //using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var refreshToken = await GetRefreshTokenAsync(context, token);
            if (refreshToken == null)
            {
                return new AuthenticationResponse(false, "Refresh token is invalid.");
            }

            var user = refreshToken.User;

            if (refreshToken.DateRevokedUtc != null)
            {
                // revoke all descendant tokens in case this token has been compromised
                RevokeDescendantRefreshTokens(refreshToken, user, $"Attempted reuse of revoked ancestor token: {token}");
            }

            if (refreshToken.DateRevokedUtc != null && DateTime.UtcNow >= refreshToken.DateExpiredUtc)
            {
                return new AuthenticationResponse(false, "Refresh token is revoked or expired.");
            }

            // replace old refresh token with a new one (rotate token)
            var (newRefreshToken, newImagesToken) = RotateRefreshToken(refreshToken);
            context.UserTokens.Add(new Data.Model.UserToken
            {
                TokenTypeId = TokenTypes.RefreshToken,
                Token = newRefreshToken.Token,
                DateExpiredUtc = newRefreshToken.Expires,
                DateRevokedUtc = newRefreshToken.Revoked,
                UserId = user.UserId,
                OrganizationId = user.OrganizationId,
                Ip = _requestContext.GetIp()
            });
            context.UserTokens.Add(new Data.Model.UserToken
            {
                TokenTypeId = TokenTypes.ImagesToken,
                Token = newImagesToken,
                DateExpiredUtc = newRefreshToken.Expires,
                DateRevokedUtc = newRefreshToken.Revoked,
                UserId = user.UserId,
                OrganizationId = user.OrganizationId,
                Ip = _requestContext.GetIp()
            });

            // remove old refresh tokens from user
            await RemoveOldUserTokensAsync(context, user);

            // update the last login time, we consider a token refresh a login
            user.DateLastLoginUtc = DateTime.UtcNow;

            // save changes to db
            var success = await context.SaveChangesAsync();
            //await transaction.CommitAsync();

            // generate new jwt
            var userContext = Map(user);
            var jwtToken = _jwt.GenerateJwtToken(userContext);
            var imagesToken = _jwt.GenerateImagesToken();

            return new AuthenticationResponse(userContext, new AuthenticatedTokens
            {
                IsAuthenticated = true,
                CanLogin = userContext.CanLogin,
                JwtToken = jwtToken,
                ImagesToken = imagesToken,
                RefreshToken = newRefreshToken.Token,
                DateCreated = newRefreshToken.Created,
                DateExpires = newRefreshToken.Expires
            });
        }

        public async Task RevokeTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var refreshToken = await GetRefreshTokenAsync(context, token);

                if (refreshToken == null || DateTime.UtcNow >= refreshToken?.DateExpiredUtc == true)
                    throw new AuthenticationException("Invalid refresh token");

                // revoke token and save
                RevokeRefreshToken(refreshToken, "Revoked without replacement");
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserContext?> GetUserAsync(int userId)
        {
            if (userId == 0) throw new ArgumentNullException(nameof(userId));
            using var context = await _contextFactory.CreateDbContextAsync();
            var userContext = await context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            return userContext != null ? Map(userContext) : null;
        }

        public async Task<RegisterNewAccountResponse> RegisterNewAccountAsync(RegisterNewAccountRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // check if account exists
                if (context.Users.Any(x => x.EmailAddress == request.Email))
                {
                    _logger.LogWarning($"[{nameof(RegisterNewAccountAsync)}] A user with that email address is already registered. Email: {request.Email} Name: {request.Name} IP: {_requestContext.GetIpAddress()}");
                    return new RegisterNewAccountResponse
                    {
                        Message = "A user with that email address is already registered."
                    };
                }
                // register account
                var user = new Data.Model.User
                {
                    OrganizationId = 1,
                    Name = request.Name,
                    EmailAddress = request.Email,
                    EmailConfirmationToken = ConfirmationTokenGenerator.NewToken(),
                    IsEmailConfirmed = false,
                    DateEmailConfirmedUtc = null,
                    IsEmailSubscribed = true,
                    PasswordHash = PasswordHasher.GeneratePasswordHash(request.Password).ToString(),
                    Ip = _requestContext.GetIp(),
                };
                context.Users.Add(user);
                if (await context.SaveChangesAsync() > 0)
                {
                    var userContext = Map(user);
                    // create authentication tokens that identify the user, but don't allow them to login
                    var authenticatedTokens = await GetAuthenticatedTokensAsync(context, userContext);
                    context.UserTokens.Add(new Data.Model.UserToken
                    {
                        Token = authenticatedTokens.RefreshToken,
                        DateCreatedUtc = authenticatedTokens.DateCreated,
                        DateExpiredUtc = authenticatedTokens.DateExpires,
                        TokenTypeId = TokenTypes.RefreshToken,
                        UserId = user.UserId,
                        OrganizationId = user.OrganizationId,
                        Ip = _requestContext.GetIp()
                    });
                    context.UserTokens.Add(new Data.Model.UserToken
                    {
                        Token = authenticatedTokens.ImagesToken,
                        DateCreatedUtc = authenticatedTokens.DateCreated,
                        DateExpiredUtc = authenticatedTokens.DateExpires,
                        TokenTypeId = TokenTypes.ImagesToken,
                        UserId = user.UserId,
                        OrganizationId = user.OrganizationId,
                        Ip = _requestContext.GetIp()
                    });
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"[{nameof(RegisterNewAccountAsync)}] New user registered! Email: {request.Email} UserId: {user.UserId} Name: {request.Name} IP: {_requestContext.GetIpAddress()}");

                    // create JWT tokens, but they won't be able to access anything until email is verified
                    return new RegisterNewAccountResponse(await GetAuthenticatedTokensAsync(context, userContext))
                    {
                        IsSuccessful = true,
                        CanLogin = false,
                        Message = "Please check your email and confirm your email address. After confirmation you will be able to login.",
                        Id = user.UserId,
                        Name = user.Name,
                        User = Map(user)
                    };
                }

                _logger.LogError($"[{nameof(RegisterNewAccountAsync)}] Failed to save new user! Email: {request.Email} Name: {request.Name} IP: {_requestContext.GetIpAddress()}");
                return new RegisterNewAccountResponse()
                {
                    IsSuccessful = false,
                    Message = "Failed to save new user!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(RegisterNewAccountAsync)}] Unhandled exception '{ex.Message}'! Email: {request.Email} Name: {request.Name} IP: {_requestContext.GetIpAddress()}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PasswordRecoveryResponse> SendPasswordResetRequest(PasswordRecoveryRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.EmailAddress == request.EmailAddress);
            if (user == null)
            {
                _logger.LogWarning($"[{nameof(SendPasswordResetRequest)}] The email address you specified does not have an account. Email: {request.EmailAddress} IP: {_requestContext.GetIpAddress()}'");
                return new PasswordRecoveryResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "The email address you specified does not have an account."
                };
            }

            var passwordResetToken = await context.UserTokens
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.UserId
                    && x.TokenTypeId == TokenTypes.PasswordResetToken
                    && x.DateExpiredUtc < DateTime.UtcNow
                    );

            if (passwordResetToken == null)
            {
                // if a valid reset token doesn't already exist, create a new one
                passwordResetToken = new Data.Model.UserToken
                {
                    TokenTypeId = TokenTypes.PasswordResetToken,
                    DateExpiredUtc = DateTime.UtcNow.AddDays(1),
                    Token = ConfirmationTokenGenerator.NewToken(),
                    User = user,
                    Ip = _requestContext.GetIp()
                };
                context.UserTokens.Add(passwordResetToken);
                await context.SaveChangesAsync();
            }

            return new PasswordRecoveryResponse
            {
                IsSuccessful = true
            };
        }

        public async Task<PasswordRecoveryResponse> ValidatePasswordResetTokenAsync(ConfirmPasswordRecoveryRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.EmailAddress == request.EmailAddress);
            if (user == null)
            {
                _logger.LogWarning($"[{nameof(ValidatePasswordResetTokenAsync)}] The password recovery token provided is invalid or expired (No user with email). Email: {request.EmailAddress} IP: {_requestContext.GetIpAddress()}");
                return new PasswordRecoveryResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "The password recovery token provided is invalid or expired."
                };
            }

            var passwordResetToken = await context.UserTokens
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.UserId
                    && x.TokenTypeId == TokenTypes.PasswordResetToken
                    && x.DateExpiredUtc > DateTime.UtcNow
                    && x.Token == request.Token
                    );

            if (passwordResetToken == null)
            {
                _logger.LogWarning($"[{nameof(ValidatePasswordResetTokenAsync)}] The password recovery token provided is invalid or expired (No passwordResetToken). Email: {request.EmailAddress} IP: {_requestContext.GetIpAddress()}");
                return new PasswordRecoveryResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "The password recovery token provided is invalid or expired."
                };
            }

            return new PasswordRecoveryResponse
            {
                IsSuccessful = true
            };
        }

        public async Task<AuthenticationResponse> ResetPasswordUsingTokenAsync(PasswordRecoverySetNewPasswordRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.EmailAddress == request.EmailAddress);
            if (user == null)
            {
                _logger.LogWarning($"[{nameof(ResetPasswordUsingTokenAsync)}] The password recovery token provided is invalid or expired (No passwordResetToken). Email: {request.EmailAddress} IP: {_requestContext.GetIpAddress()}");
                return new AuthenticationResponse(false, "The password recovery token provided is invalid or expired.");
            }

            var passwordResetToken = await context.UserTokens
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.UserId
                    && x.TokenTypeId == TokenTypes.PasswordResetToken
                    && x.DateExpiredUtc > DateTime.UtcNow
                    && x.Token == request.Token
                    );

            if (passwordResetToken == null)
            {
                _logger.LogWarning($"[{nameof(ResetPasswordUsingTokenAsync)}] The password recovery token provided is invalid or expired (No passwordResetToken). Email: {request.EmailAddress} IP: {_requestContext.GetIpAddress()}");
                return new AuthenticationResponse(false, "The password recovery token provided is invalid or expired.");
            }

            // validate password, save new password
            if (!request.Password.Equals(request.ConfirmPassword))
            {
                _logger.LogWarning($"[{nameof(ResetPasswordUsingTokenAsync)}] The passwords provided do not match. Please check you typed it correctly. Email: {request.EmailAddress} IP: {_requestContext.GetIpAddress()}");
                return new AuthenticationResponse(false, "The passwords provided do not match. Please check you typed it correctly.");
            }

            // save new password
            user.PasswordHash = PasswordHasher.GeneratePasswordHash(request.Password).ToString();
            user.LastSetPasswordIp = _requestContext.GetIp();

            var isSuccessful = await context.SaveChangesAsync() > 0;

            if (isSuccessful)
            {
                // remove the password reset token so it can't be used again
                context.UserTokens.Remove(passwordResetToken);
                await context.SaveChangesAsync();

                // login the user
                var userContext = Map(user);
                var authenticationResponse = await CreateAuthenticationLoginAsync(context, user, userContext);

                return authenticationResponse;
            }

            _logger.LogWarning($"[{nameof(ResetPasswordUsingTokenAsync)}] An unknown error occurred, your new password was not saved. Email: {request.EmailAddress} IP: {_requestContext.GetIpAddress()}");
            return new AuthenticationResponse(false, "An unknown error occurred, your new password was not saved.");
        }

        public async Task<ClaimsPrincipal> SetCurrentUserFromIdAsync(int userId)
        {
            var user = await GetUserAsync(userId);
            var claims = _jwt.GetClaims(user);
            var claimsIdentity = new ClaimsIdentity(claims, "Password");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            Thread.CurrentPrincipal = claimsPrincipal;
            _requestContext.SetUser(claimsPrincipal);
            return claimsPrincipal;
        }
        
        private async Task<AuthenticatedTokens> GetAuthenticatedTokensAsync(BinnerContext context, UserContext userContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            // login allowed, issue a jwt access token & refresh token
            var jwtToken = _jwt.GenerateJwtToken(userContext);
            var imagesToken = _jwt.GenerateImagesToken();
            var refreshToken = _jwt.GenerateRefreshToken();

            return new AuthenticatedTokens
            {
                IsAuthenticated = !string.IsNullOrWhiteSpace(jwtToken),
                CanLogin = userContext.CanLogin,
                JwtToken = jwtToken,
                ImagesToken = imagesToken,
                RefreshToken = refreshToken.Token,
                DateCreated = refreshToken.Created,
                DateExpires = refreshToken.Expires
            };
        }

        private async Task<Data.Model.UserToken?> GetRefreshTokenAsync(BinnerContext context, string token)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var userToken = await context.UserTokens
                .Include(x => x.User)
                .SingleOrDefaultAsync(t => t.Token == token
                    && t.TokenTypeId == TokenTypes.RefreshToken);
            return userToken;
        }

        private (RefreshToken, string? imagesToken) RotateRefreshToken(Data.Model.UserToken refreshToken)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));
            var newRefreshToken = _jwt.GenerateRefreshToken();
            var newImagesToken = _jwt.GenerateImagesToken();
            RevokeRefreshToken(refreshToken, newRefreshToken.Token);
            return (newRefreshToken, newImagesToken);
        }

        private async Task RemoveOldUserTokensAsync(BinnerContext context, Data.Model.User user)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (user == null) throw new ArgumentNullException(nameof(user));
            // remove old inactive refresh tokens from user based on TTL in app settings
            var expiryTime = DateTime.UtcNow;
            var expiredTokens = await context.UserTokens
                .Where(x => x.UserId == user.UserId
                    && x.DateExpiredUtc != null
                    && x.DateRevokedUtc != null
                    && expiryTime >= x.DateExpiredUtc)
                .ToListAsync();
            context.UserTokens.RemoveRange(expiredTokens);
        }

        private void RevokeDescendantRefreshTokens(Data.Model.UserToken refreshToken, Data.Model.User user, string reason)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));
            if (user == null) throw new ArgumentNullException(nameof(user));
            // recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = user.UserTokens
                    .SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken
                        && x.TokenTypeId == TokenTypes.RefreshToken);
                if (childToken != null && childToken.DateRevokedUtc != null && DateTime.UtcNow <= childToken.DateExpiredUtc)
                    RevokeRefreshToken(childToken, reason);
                else if (childToken != null)
                    RevokeDescendantRefreshTokens(childToken, user, reason);
            }
        }

        private void RevokeRefreshToken(Data.Model.UserToken token, string? replacedByToken = null)
        {
            token.DateRevokedUtc = DateTime.UtcNow;
            token.ReplacedByToken = replacedByToken;
        }

        private UserContext Map(Data.Model.User user)
        {
            return new UserContext
            {
                UserId = user.UserId,
                OrganizationId = user.OrganizationId,
                Name = user.Name,
                EmailAddress = user.EmailAddress,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                CanLogin = user.IsEmailConfirmed && user.DateLockedUtc == null,
                IsAdmin = user.IsAdmin
            };
        }
    }
}
