using ApiClient.OAuth2;
using Binner.Common;
using Binner.Common.Integrations;
using Binner.Common.Integrations.Models.Digikey;
using Binner.Common.Services;
using Binner.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AuthorizationController : ControllerBase
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly OAuth2Service _oAuth2Service;
        private readonly ICredentialService _credentialService;

        public AuthorizationController(ILogger<AuthorizationController> logger, OAuth2Service oAuth2Service, ICredentialService credentialService)
        {
            _logger = logger;
            _oAuth2Service = oAuth2Service;
            _credentialService = credentialService;
        }

        /// <summary>
        /// Authorize an oAuth application
        /// </summary>
        /// <remarks>
        /// This is used as a postback Url for oAuth authorization of external services/integrations
        /// </remarks>
        /// <param name="code"></param>
        /// <param name="scope"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AuthorizeAsync([FromQuery]string code, [FromQuery]string scope, [FromQuery]string state)
        {
            var contextKey = $"{nameof(DigikeyApi)}-{User.Identity?.Name}";
            var authRequest = ServerContext.Get<OAuthAuthorization>(contextKey);
            if (authRequest == null)
            {
                // try get next integration
                contextKey = $"{nameof(MouserApi)}-{User.Identity?.Name}";
                authRequest = ServerContext.Get<OAuthAuthorization>(contextKey);
            }
            if (authRequest != null)
            {
                var authResult = await _oAuth2Service.FinishAuthorization(code);
                // store the authorization tokens
                // result.AccessToken
                // result.RefreshToken
                // result.ExpiresIn
                authRequest.AuthorizationReceived = true;
                if (authResult.IsError)
                {
                    authRequest.Error = authResult.Error;
                    authRequest.ErrorDescription = authResult.ErrorDescription;
                }
                else
                {
                    authRequest.AccessToken = authResult.AccessToken;
                    authRequest.RefreshToken = authResult.RefreshToken;
                    authRequest.CreatedUtc = DateTime.UtcNow;
                    authRequest.ExpiresUtc = DateTime.UtcNow.Add(TimeSpan.FromSeconds(authResult.ExpiresIn));
                }

                ServerContext.Set(contextKey, authRequest);
                // save the credential
                await _credentialService.SaveOAuthCredentialAsync(new OAuthCredential
                {
                    Provider = contextKey,
                    AccessToken = authRequest.AccessToken,
                    RefreshToken = authRequest.RefreshToken,
                    DateCreatedUtc = authRequest.CreatedUtc,
                    DateExpiresUtc = authRequest.ExpiresUtc,
                });

                return Redirect(authRequest.ReturnToUrl);
            }
            return BadRequest("No authorization request found, invalid callback.");
        }
    }
}
