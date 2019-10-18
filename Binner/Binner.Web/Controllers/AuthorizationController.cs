using ApiClient.OAuth2;
using Binner.Common;
using Binner.Common.Integrations.Models.Digikey;
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
        public AuthorizationController(ILogger<AuthorizationController> logger, OAuth2Service oAuth2Service)
        {
            _logger = logger;
            _oAuth2Service = oAuth2Service;
        }

        /// <summary>
        /// Authorize an oAuth application
        /// </summary>
        /// <remarks>
        /// This is used for Digikey oAuth authorization
        /// </remarks>
        /// <param name="code"></param>
        /// <param name="scope"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AuthorizeAsync([FromQuery]string code, [FromQuery]string scope, [FromQuery]string state)
        {
            // todo: generalize this so its not Api specific
            var authRequest = ServerContext.Get<DigikeyAuthorization>(nameof(DigikeyAuthorization));
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

                ServerContext.Set(nameof(DigikeyAuthorization), authRequest);

                return Ok("Authorization successful! You can now proceed to use integrations.");
            }
            return BadRequest("No authorization request found, invalid callback.");
        }
    }
}
