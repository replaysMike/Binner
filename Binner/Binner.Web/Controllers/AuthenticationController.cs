using Binner.Services;
using Binner.Global.Common;
using Binner.Model.Configuration;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    [Consumes(MediaTypeNames.Application.Json)]
    public partial class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly AuthenticationConfiguration _configuration;
        private readonly IRequestContextAccessor _requestContextAccessor;

        public AuthenticationController(ILogger<AuthenticationController> logger, IAuthenticationService authenticationService, AuthenticationConfiguration configuration, IRequestContextAccessor requestContextAccessor)
        {
            _logger = logger;
            _authenticationService = authenticationService;
            _configuration = configuration;
            _requestContextAccessor = requestContextAccessor;
        }

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterNewAccountRequest request)
        {
            try
            {
                var response = await _authenticationService.RegisterNewAccountAsync(request);
                if (response.IsAuthenticated && !string.IsNullOrEmpty(response.RefreshToken))
                {
                    SetTokenCookie(response.RefreshToken);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Error registering account.", ex));
            }
        }

        /// <summary>
        /// Authenticate password credentials
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(AuthenticationRequest request)
        {
            try
            {
                // validate credentials & issue jwt
                var response = await _authenticationService.AuthenticateAsync(request);
                if (response.IsAuthenticated && !string.IsNullOrEmpty(response.RefreshToken))
                {
                    SetTokenCookie(response.RefreshToken);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Error attempting to authenticate.", ex));
            }
        }

        /// <summary>
        /// Logout the user
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            DeleteTokenCookie();
            return Ok(new { message = "Ok" });
        }

        /// <summary>
        /// Submit an email address for password recovery
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("passwordrecovery")]
        public async Task<IActionResult> PasswordRecoveryAsync(PasswordRecoveryRequest request)
        {
            try
            {
                var response = await _authenticationService.SendPasswordResetRequest(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Error creating password recovery request.", ex));
            }
        }

        /// <summary>
        /// Check if a password recovery token is valid
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("passwordrecovery/validate")]
        public async Task<IActionResult> ValidatePasswordRecoveryTokenAsync(ConfirmPasswordRecoveryRequest request)
        {
            try
            {
                var response = await _authenticationService.ValidatePasswordResetTokenAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Error validating password recovery token.", ex));
            }
        }

        /// <summary>
        /// Reset a user's password using a password reset token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("passwordrecovery/resetpassword")]
        public async Task<IActionResult> ResetPasswordUsingTokenAsync(PasswordRecoverySetNewPasswordRequest request)
        {
            try
            {
                var response = await _authenticationService.ResetPasswordUsingTokenAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Error resetting new user password.", ex));
            }
        }

        /// <summary>
        /// Get the current user identity
        /// </summary>
        /// <returns></returns>
        [HttpGet("identity")]
        public IActionResult GetIdentity()
        {
            return Ok(_requestContextAccessor.GetUserContext());
        }

        /// <summary>
        /// Get a new refresh token
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync()
        {
            try
            {
                // refresh token should only ever be in a http-only cookie
                var refreshToken = Request.Cookies["refreshToken"];
                if (refreshToken == null)
                    return Unauthorized();
                var response = await _authenticationService.RefreshTokenAsync(refreshToken);

                if (response.IsAuthenticated && !string.IsNullOrEmpty(response.RefreshToken))
                    SetTokenCookie(response.RefreshToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Error attempting to refresh token.", ex));
            }
        }

        /// <summary>
        /// Get a new refresh token
        /// </summary>
        /// <returns></returns>
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeTokenAsync()
        {
            try
            {
                // accept refresh token in request header or cookie
                var refreshToken = Request.Headers["refreshToken"].FirstOrDefault() ?? Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                    return BadRequest("refreshToken is required");
                await _authenticationService.RevokeTokenAsync(refreshToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Error attempting to revoke token.", ex));
            }
        }

        private void SetTokenCookie(string token)
        {
            // append cookie with refresh token to the http response
            var cookieOptions = new CookieOptions
            {
                // very important that HttpOnly=true. Javascript should not have access to this cookie
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                // Secure = true, todo: enable when SSL is enabled
                Expires = DateTime.UtcNow.Add(_configuration.JwtRefreshTokenExpiryTime)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private void DeleteTokenCookie()
        {
            Response.Cookies.Delete("refreshToken");
        }
    }
}
