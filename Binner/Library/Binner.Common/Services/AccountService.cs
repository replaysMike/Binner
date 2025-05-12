using AutoMapper;
using Binner.Common.Extensions;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataModel = Binner.Data.Model;

namespace Binner.Common.Services
{
    /// <summary>
    /// Manage users
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly IRequestContextAccessor _requestContext;
        private readonly IConfiguration _configuration;

        public AccountService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IRequestContextAccessor requestContext, IConfigurationRoot configuration)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
            _configuration = configuration;
        }

        private IQueryable<DataModel.User> GetAccountQueryable(BinnerContext context)
            => context.Users
                .AsQueryable();

        public async Task<Account?> GetAccountAsync()
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetAccountQueryable(context)
                .Include(x => x.UserTokens)
                .Where(x => x.UserId == userContext.UserId && x.OrganizationId == userContext.OrganizationId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity != null)
            {
                // filter out tokens we want
                entity.UserTokens = entity.UserTokens?.Where(x => x.TokenTypeId == Model.Authentication.TokenTypes.KiCadApiToken).ToList();
                var model = _mapper.Map<Account>(entity);
                model.PartsInventoryCount = await context.Parts.CountAsync(x => x.OrganizationId == userContext.OrganizationId);
                model.PartTypesCount = await context.PartTypes.CountAsync(x => x.OrganizationId == userContext.OrganizationId);
                return model;
            }

            return null;
        }

        public async Task<UpdateAccountResponse> UpdateAccountAsync(Account account)
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetAccountQueryable(context)
                .Where(x => x.UserId == userContext.UserId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return new UpdateAccountResponse
                {
                    Account = _mapper.Map<Account>(entity),
                    IsSuccessful = false,
                    Message = $"Could not find user with id '{userContext.UserId}'",
                };
            }

            if (entity.EmailAddress != account.EmailAddress)
            {
                // user wants to change login email/username. Make sure it's not already in the system.
                var existingEmails = await GetAccountQueryable(context)
                .Where(x => x.EmailAddress == account.EmailAddress && x.OrganizationId == userContext.OrganizationId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();
                if (existingEmails != null)
                {
                    return new UpdateAccountResponse
                    {
                        Account = _mapper.Map<Account>(entity),
                        IsSuccessful = false,
                        Message = $"A user with this email already exists.",
                    };
                }
            }

            if (!string.IsNullOrEmpty(account.NewPassword))
            {
                if (!PasswordHasher.Verify(account.Password, entity.PasswordHash))
                {
                    return new UpdateAccountResponse
                    {
                        Account = _mapper.Map<Account>(entity),
                        IsSuccessful = false,
                        Message = "Incorrect password.",
                    };
                }

                if (!account.NewPassword.Equals(account.ConfirmNewPassword))
                    return new UpdateAccountResponse
                    {
                        Account = _mapper.Map<Account>(entity),
                        IsSuccessful = false,
                        Message = "Passwords do not match.",
                    };

                // change user's password
                entity.PasswordHash = PasswordHasher.GeneratePasswordHash(account.NewPassword).ToString();
            }

            account.Tokens = null; // ensure no tokens can be updated
            entity = _mapper.Map(account, entity);

            await context.SaveChangesAsync();

            return new UpdateAccountResponse
            {
                Account = _mapper.Map<Account>(entity),
                IsSuccessful = true
            };
        }

        public async Task UploadProfileImageAsync(MemoryStream stream, string originalFilename, string contentType, long length)
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetAccountQueryable(context)
                .Where(x => x.UserId == userContext.UserId && x.OrganizationId == userContext.OrganizationId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null)
                throw new KeyNotFoundException($"Could not find user with id '{userContext.UserId}'");

            // convert image to png
            try
            {
                var image = Bitmap.FromStream(stream);
                using var outputStream = new MemoryStream();
                image.Save(outputStream, ImageFormat.Png);

                entity.ProfileImage = outputStream.ToArray();
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to save profile image", ex);
            }
        }

        public async Task<Token?> CreateKiCadApiTokenAsync(string? tokenConfig)
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserTokens
                .Where(x => x.UserId == userContext.UserId
                    && x.OrganizationId == userContext.OrganizationId
                    && x.TokenTypeId == Model.Authentication.TokenTypes.KiCadApiToken)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            // delete any existing api token. user can only have 1
            if (entity != null)
            {
                context.UserTokens.Remove(entity);
            }

            var newToken = new DataModel.UserToken()
            {
                Token = TokenGenerator.NewToken(),
                TokenConfig = tokenConfig,
                DateCreatedUtc = DateTime.UtcNow,
                DateExpiredUtc = null,
                TokenTypeId = Model.Authentication.TokenTypes.KiCadApiToken,
                UserId = userContext.UserId,
                OrganizationId = userContext.OrganizationId,
                Ip = _requestContext.GetIp()
            };
            context.UserTokens.Add(newToken);
            await context.SaveChangesAsync();

            return _mapper.Map<DataModel.UserToken, Token>(newToken);
        }

        public async Task<bool> DeleteKiCadApiTokenAsync(string token)
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserTokens
                .Where(x => x.UserId == userContext.UserId
                    && x.OrganizationId == userContext.OrganizationId
                    && x.TokenTypeId == Model.Authentication.TokenTypes.KiCadApiToken
                    && x.Token == token)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null) return false;

            context.UserTokens.Remove(entity);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<Token?> GetTokenAsync(string token, Model.Authentication.TokenTypes? tokenType = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var apiToken = await context.UserTokens
                .WhereIf(tokenType != null, x => x.TokenTypeId == tokenType)
                .Where(x =>
                    x.DateRevokedUtc == null
                    && x.Token == token
                    && (x.DateExpiredUtc == null || x.DateExpiredUtc > DateTime.UtcNow))
                .FirstOrDefaultAsync();

            if (apiToken != null)
                return _mapper.Map<Token>(apiToken);

            return null;
        }

        public async Task<bool> ValidateKiCadApiToken(string token)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var apiToken = await context.UserTokens
                .FirstOrDefaultAsync(x =>
                    x.TokenTypeId == Model.Authentication.TokenTypes.KiCadApiToken
                    && x.DateRevokedUtc == null
                    && x.Token == token
                    && (x.DateExpiredUtc == null || x.DateExpiredUtc > DateTime.UtcNow));

            if (apiToken != null)
                return true;

            return false;
        }
    }
}
