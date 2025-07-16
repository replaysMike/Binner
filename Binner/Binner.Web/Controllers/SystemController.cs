using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Responses;
using Binner.Services;
using Binner.Services.IO;
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
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly ISettingsService _settingsService;
        private readonly IVersionManagementService _versionManagementService;
        private readonly IBackupProvider _backupProvider;
        private readonly IAdminService _adminService;

        public SystemController(ILogger<SystemController> logger, WebHostServiceConfiguration config, ISettingsService settingsService, IVersionManagementService versionManagementService, IBackupProvider backupProvider, IAdminService adminService)
        {
            _logger = logger;
            _config = config;
            _settingsService = settingsService;
            _versionManagementService = versionManagementService;
            _backupProvider = backupProvider;
            _adminService = adminService;
        }

        [AllowAnonymous]
        [HttpGet("/api/ping")]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var result = await _settingsService.PingDatabaseAsync();
                if (!result) return StatusCode(500, "Failed to connect to database!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to connect to database due to exception! {ex.GetBaseException().Message}");
            }
            return Ok("pong");
        }

        /// <summary>
        /// Get information about the Binner installation
        /// </summary>
        /// <returns></returns>
        [HttpGet("info")]
        [Authorize(Policy = Binner.Model.Authentication.AuthorizationPolicies.Admin)]
        public async Task<IActionResult> GetSystemInformationAsync()
        {
            try
            {
                var result = await _adminService.GetSystemInfoAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Get system info error! ", ex));
            }
        }

        /// <summary>
        /// Get the latest Binner version info available
        /// </summary>
        /// <returns></returns>
        [HttpGet("version")]
        public async Task<IActionResult> GetVersionAsync()
        {
            try
            {
                var result = await _versionManagementService.GetLatestVersionAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Get version error! ", ex));
            }
        }

        /// <summary>
        /// Get the latest Binner system messages available
        /// </summary>
        /// <returns></returns>
        [HttpGet("messages")]
        public async Task<IActionResult> GetSystemMessagesAsync()
        {
            try
            {
                var result = await _versionManagementService.GetSystemMessagesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Get System Messages error! ", ex));
            }
        }

        /// <summary>
        /// Update the latest Binner system messages read by the user
        /// </summary>
        /// <returns></returns>
        [HttpPut("messages/read")]
        public async Task<IActionResult> ReadSystemMessagesAsync(UpdateSystemMessagesRequest request)
        {
            try
            {
                await _versionManagementService.UpdateSystemMessagesReadAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Update System Messages error! ", ex));
            }
        }

        /// <summary>
        /// Perform a complete backup of Binner
        /// </summary>
        /// <returns></returns>
        [HttpPost("backup")]
        [Authorize(Policy = Binner.Model.Authentication.AuthorizationPolicies.Admin)]
        public async Task<IActionResult> BackupAsync()
        {
            try
            {
                var stream = await _backupProvider.BackupAsync();
                var now = DateTime.UtcNow;
                return File(stream, "application/octet-stream", $"Binner-{now.Year}-{now.Month}-{now.Day}.bak");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ExceptionResponse(ex));
            }
        }

        /// <summary>
        /// Perform a complete restore of Binner
        /// </summary>
        /// <returns></returns>
        [HttpPost("restore")]
        [Consumes("multipart/form-data")]
        [Authorize(Policy = Binner.Model.Authentication.AuthorizationPolicies.Admin)]
        public async Task<IActionResult> RestoreAsync(IFormFile file)
        {
            try
            {
                var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;
                var uploadFile = new UploadFile(file.FileName, stream);
                await _backupProvider.RestoreAsync(uploadFile);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ExceptionResponse(ex));
            }
        }
    }
}
