using Binner.Common.Services;
using Binner.Model;
using Binner.Model.Authentication;
using Binner.Model.Configuration;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IAccountService _accountService;

        public AccountController(ILogger<AccountController> logger, WebHostServiceConfiguration config, IAccountService accountService)
        {
            _logger = logger;
            _config = config;
            _accountService = accountService;
        }

        /// <summary>
        /// Get the user's account information
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAccountAsync()
        {
            var user = await _accountService.GetAccountAsync();
            if (user != null)
                user.IPAddress = Request.HttpContext.Connection?.RemoteIpAddress?.ToString();
            return Ok(user);
        }

        /// <summary>
        /// Create an api token
        /// </summary>
        /// <returns></returns>
        [HttpPost("token")]
        public async Task<IActionResult> CreateTokenAsync(CreateTokenRequest request)
        {
            switch(request.TokenType)
            {
                case TokenTypes.KiCadApiToken:
                    var token = await _accountService.CreateKiCadApiTokenAsync();
                    return Ok(token);
                default:
                    return BadRequest("Unsupported token type");
            }
        }

        /// <summary>
        /// Delete an api token
        /// </summary>
        /// <returns></returns>
        [HttpDelete("token")]
        public async Task<IActionResult> DeleteTokenAsync(DeleteTokenRequest request)
        {
            switch (request.TokenType)
            {
                case TokenTypes.KiCadApiToken:
                    var token = await _accountService.DeleteKiCadApiTokenAsync(request.Value);
                    return Ok(token);
                default:
                    return BadRequest("Unsupported token type");
            }
        }

        /// <summary>
        /// Update the user's account information
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateAccountAsync(Account request)
        {
            var user = await _accountService.UpdateAccountAsync(request);
            user.Account.IPAddress = Request.HttpContext.Connection?.RemoteIpAddress?.ToString();
            return Ok(user);
        }

        /// <summary>
        /// Upload a profile image
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> OnUploadProfileImageAsync(IFormFile file)
        {
            try
            {
                if (file != null)
                {
                    using var stream = new MemoryStream();
                    await file.CopyToAsync(stream);
                    // store the resource in the repository
                    await _accountService.UploadProfileImageAsync(stream, file.FileName, file.ContentType, file.Length);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse(ex.Message, ex));
            }
        }
    }
}
