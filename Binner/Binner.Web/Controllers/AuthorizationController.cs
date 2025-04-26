using Binner.Common.Integrations;
using Binner.Common.Services;
using Binner.Common.Services.Authentication;
using Binner.Model;
using Binner.Model.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AuthorizationController : ControllerBase
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly ICredentialService _credentialService;
        private readonly JwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IIntegrationApiFactory _integrationApiFactory;

        public AuthorizationController(ILogger<AuthorizationController> logger, ICredentialService credentialService, IIntegrationApiFactory integrationApiFactory, IUserService userService, JwtService jwtService)
        {
            _logger = logger;
            _credentialService = credentialService;
            _jwtService = jwtService;
            _integrationApiFactory = integrationApiFactory;
            _userService = userService;
        }

        /// <summary>
        /// Authorize an oAuth application
        /// </summary>
        /// <remarks>
        /// This is used as a postback Url for oAuth authorization of external services/integrations.
        /// Currently only DigiKey supports this (and is required for using their api)
        /// </remarks>
        /// <param name="code">The authorization code provided by the api</param>
        /// <param name="scope">Optional, Scope is unused</param>
        /// <param name="state">State should be a Guid indicating the dbo.OAuthRequests.RequestId</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AuthorizeAsync([FromQuery] string? code, [FromQuery] string? scope, [FromQuery] string? state)
        {
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError($"[{nameof(AuthorizeAsync)}] Code parameter must be specified");
                throw new ArgumentNullException(nameof(code), "Code parameter must be specified");
            }
            if (string.IsNullOrEmpty(state))
            {
                _logger.LogError($"[{nameof(AuthorizeAsync)}] State parameter must be specified");
                throw new ArgumentNullException(nameof(state), "State parameter must be specified");
            }

            // state is a Guid indicating the requestId
            if (!Guid.TryParse(state, out var requestId) && requestId != Guid.Empty)
            {
                _logger.LogError($"[{nameof(AuthorizeAsync)}] Error - State is an invalid format.");
                throw new ArgumentException("State is an invalid format.", nameof(state));
            }

            // get the original oauth request from the stored requestId
            var authRequest = await _credentialService.GetOAuthRequestAsync(requestId, false);
            if (authRequest == null)
            {
                _logger.LogError($"[{nameof(AuthorizeAsync)}] Error - no originating oAuth request found! You must restart the oAuth authentication process.");
                throw new AuthenticationException($"Error - no originating oAuth request found! You must restart the oAuth authentication process.");
            }

            if (authRequest.UserId == null)
            {
                _logger.LogError($"[{nameof(AuthorizeAsync)}] Error - no user is associated with this oAuth request!");
                throw new AuthenticationException($"Error - no user is associated with this oAuth request!");
            }

            switch (authRequest.Provider)
            {
                case nameof(DigikeyApi):
                    try
                    {
                        _logger.LogInformation($"[{nameof(AuthorizeAsync)}] Completing auth for '{authRequest.Provider}' provider");
                        await FinishDigikeyApiAuthorizationAsync(authRequest, code);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[{nameof(AuthorizeAsync)}] Failed to complete auth for '{authRequest.Provider}' provider");
                        return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Failed to authenticate with DigiKey due to an error! ", ex));
                    }
                    break;
                default:
                    _logger.LogError($"[{nameof(AuthorizeAsync)}] Unhandled OAuth provider name '{authRequest.Provider}' provider");
                    throw new NotImplementedException($"Unhandled OAuth provider name '{authRequest.Provider}'");
            }

            return Redirect(authRequest.ReturnToUrl);
        }

        private async Task FinishDigikeyApiAuthorizationAsync(OAuthAuthorization authRequest, string code)
        {
            // local binner does not require a valid userId
            var userId = authRequest.UserId ?? 0;
            var digikeyApi = await _integrationApiFactory.CreateAsync<DigikeyApi>(userId);
            _logger.LogInformation($"[{nameof(FinishDigikeyApiAuthorizationAsync)}] Completing OAuth DigiKey authorization for user {userId}");
            var authResult = await digikeyApi.OAuth2Service.FinishAuthorization(code);

            if (authResult == null || authResult.IsError)
            {
                var error = authResult?.ErrorMessage ?? "No auth result received";
                var errorDescription = authResult?.ErrorDetails ?? string.Empty;
                _logger.LogError($"[{nameof(FinishDigikeyApiAuthorizationAsync)}] Failed to complete OAuth with DigiKey for user {userId}.\n{error}\n{errorDescription}");
                throw new AuthenticationException($"Failed to authenticate with DigiKey api. {error} - {errorDescription}");
            }
            else
            {
                authRequest.AuthorizationReceived = true;
                authRequest.AuthorizationCode = code;

                // reconstruct the user's session based on their UserId, since we don't have a Jwt token here for the user
                var user = await _userService.GetUserAsync(new Model.User { UserId = userId });
                if (user == null)
                {
                    _logger.LogError($"[{nameof(FinishDigikeyApiAuthorizationAsync)}] UserId '{userId}' not found!");
                    throw new AuthenticationException($"UserId '{userId}' not found!");
                }

                // create claims for the user's identity
                var claims = _jwtService.GetClaims(new Global.Common.UserContext
                {
                    EmailAddress = user.EmailAddress,
                    UserId = user.UserId,
                    OrganizationId = user.OrganizationId,
                    Name = user.Name,
                });
                var claimsIdentity = new ClaimsIdentity(claims, "Password");
                System.Threading.Thread.CurrentPrincipal = new ClaimsPrincipal(claimsIdentity);
                User.AddIdentity(claimsIdentity);

                // store the authorization tokens
                authRequest.AccessToken = authResult.AccessToken ?? string.Empty;
                authRequest.RefreshToken = authResult.RefreshToken ?? string.Empty;
                authRequest.CreatedUtc = DateTime.UtcNow;
                authRequest.ExpiresUtc = DateTime.UtcNow.Add(TimeSpan.FromSeconds(authResult.ExpiresIn));

                // save the credential
                var credential = await _credentialService.SaveOAuthCredentialAsync(new OAuthCredential
                {
                    Provider = nameof(DigikeyApi),
                    AccessToken = authRequest.AccessToken,
                    RefreshToken = authRequest.RefreshToken,
                    DateCreatedUtc = authRequest.CreatedUtc,
                    DateExpiresUtc = authRequest.ExpiresUtc,
                });

                // update oauth request as complete
                await _credentialService.UpdateOAuthRequestAsync(authRequest);
                _logger.LogInformation($"[{nameof(FinishDigikeyApiAuthorizationAsync)}] Successfully completed OAuth DigiKey authorization for user {userId} with access token '{authResult.AccessToken}'!");
            }
        }
    }
}
