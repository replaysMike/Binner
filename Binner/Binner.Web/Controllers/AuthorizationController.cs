using Binner.Common.Integrations;
using Binner.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Security.Authentication;
using System.Threading.Tasks;
using Binner.Model;

namespace Binner.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AuthorizationController : ControllerBase
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly ICredentialService _credentialService;
        private readonly IIntegrationApiFactory _integrationApiFactory;

        public AuthorizationController(ILogger<AuthorizationController> logger, ICredentialService credentialService, IIntegrationApiFactory integrationApiFactory)
        {
            _logger = logger;
            _credentialService = credentialService;
            _integrationApiFactory = integrationApiFactory;
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
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code), "Code parameter must be specified");
            if (string.IsNullOrEmpty(state)) throw new ArgumentNullException(nameof(state), "State parameter must be specified");

            // state is a Guid indicating the requestId
            if (!Guid.TryParse(state, out var requestId) && requestId != Guid.Empty)
                throw new ArgumentException("State is an invalid format.", nameof(state));

            var authRequest = await _credentialService.GetOAuthRequestAsync(requestId);
            if (authRequest == null)
                throw new AuthenticationException($"Error - no originating oAuth request found! You must restart the oAuth authentication process.");

            // local binner does not require a valid userId
            //if (authRequest.UserId == null)
            //    throw new AuthenticationException($"Error - no user is associated with this oAuth request!");

            switch (authRequest.Provider)
            {
                case nameof(DigikeyApi):
                    await HandleDigikeyApiAuthorizationAsync(authRequest, code);
                    break;
                default:
                    throw new NotImplementedException($"Unhandled OAuth provider name '{authRequest.Provider}'");
            }

            return Redirect(authRequest.ReturnToUrl);
        }

        private async Task HandleDigikeyApiAuthorizationAsync(OAuthAuthorization authRequest, string code)
        {
            // local binner does not require a valid userId
            var digikeyApi = await _integrationApiFactory.CreateAsync<DigikeyApi>(authRequest.UserId ?? 0);
            var authResult = await digikeyApi.OAuth2Service.FinishAuthorization(code);

            // reconstruct the user's session based on their UserId, since we don't have a Jwt token here for the user
            //var user = await _authenticationService.GetUserAsync(authRequest.UserId.Value);
            //var claims = _jwtService.GetClaims(user);
            //var claimsIdentity = new ClaimsIdentity(claims, "Password");
            //System.Threading.Thread.CurrentPrincipal = new ClaimsPrincipal(claimsIdentity);
            //User.AddIdentity(claimsIdentity);

            authRequest.AuthorizationReceived = true;
            authRequest.AuthorizationCode = code;
            if (authResult == null || authResult.IsError)
            {
                authRequest.Error = authResult?.ErrorMessage ?? "No auth result received";
                authRequest.ErrorDescription = authResult?.ErrorDetails ?? string.Empty;
            }
            else
            {
                // store the authorization tokens
                // result.AccessToken
                // result.RefreshToken
                // result.ExpiresIn
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
            }

            // update oauth request as complete
            await _credentialService.UpdateOAuthRequestAsync(authRequest);
        }
    }
}
