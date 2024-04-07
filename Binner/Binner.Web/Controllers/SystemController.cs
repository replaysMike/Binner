using Binner.Common.Integrations;
using Binner.Common.IO;
using Binner.Common.IO.Printing;
using Binner.Common.Services;
using Binner.Global.Common;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing.PrinterHardware;
using Binner.Model.Requests;
using Binner.Model.Responses;
using LightInject;
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
        private const string AppSettingsFilename = "appsettings.json";
        private readonly ILogger<ProjectController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly ISettingsService _settingsService;
        private readonly IntegrationService _integrationService;
        private readonly ILabelPrinterHardware _labelPrinter;
        private readonly FontManager _fontManager;
        private readonly AutoMapper.IMapper _mapper;
        private readonly IServiceContainer _container;
        private readonly IIntegrationCredentialsCacheProvider _credentialProvider;
        private readonly RequestContextAccessor _requestContext;
        private readonly IVersionManagementService _versionManagementService;
        private readonly IBackupProvider _backupProvider;
        private readonly IAdminService _adminService;

        public SystemController(AutoMapper.IMapper mapper, IServiceContainer container, ILogger<ProjectController> logger, WebHostServiceConfiguration config, ISettingsService settingsService, IntegrationService integrationService, ILabelPrinterHardware labelPrinter, FontManager fontManager, RequestContextAccessor requestContextAccessor, IIntegrationCredentialsCacheProvider credentialProvider, IVersionManagementService versionManagementService, IBackupProvider backupProvider, IAdminService adminService)
        {
            _mapper = mapper;
            _container = container;
            _logger = logger;
            _config = config;
            _settingsService = settingsService;
            _integrationService = integrationService;
            _labelPrinter = labelPrinter;
            _fontManager = fontManager;
            _requestContext = requestContextAccessor;
            _credentialProvider = credentialProvider;
            _versionManagementService = versionManagementService;
            _backupProvider = backupProvider;
            _adminService = adminService;
        }

        /// <summary>
        /// Set the system settings
        /// </summary>
        /// <returns></returns>
        [HttpPut("settings")]
        public IActionResult SaveSettings(SettingsRequest request) 
        {
            try
            {
                // validate formats
                if (string.IsNullOrEmpty(request.Binner.ApiUrl)) request.Binner.ApiUrl = "https://swarm.binner.io";
                if (string.IsNullOrEmpty(request.Digikey.ApiUrl)) request.Digikey.ApiUrl = "https://api.digikey.com";
                if (string.IsNullOrEmpty(request.Digikey.oAuthPostbackUrl)) request.Digikey.oAuthPostbackUrl = "https://localhost:8090/Authorization/Authorize";
                if (string.IsNullOrEmpty(request.Mouser.ApiUrl)) request.Mouser.ApiUrl = "https://api.mouser.com";
                if (string.IsNullOrEmpty(request.Arrow.ApiUrl)) request.Arrow.ApiUrl = "https://api.arrow.com";
                request.Binner.ApiUrl = $"https://{request.Binner.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Digikey.ApiUrl = $"https://{request.Digikey.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Digikey.oAuthPostbackUrl = $"https://{request.Digikey.oAuthPostbackUrl.Replace("https://", "").Replace("http://", "")}";
                request.Mouser.ApiUrl = $"https://{request.Mouser.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Arrow.ApiUrl = $"https://{request.Arrow.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Tme.ApiUrl = $"https://{request.Tme.ApiUrl.Replace("https://", "").Replace("http://", "")}";

                // clear the credentials cache for the apis
                var user = _requestContext.GetUserContext();
                _credentialProvider.Cache.Clear(new ApiCredentialKey { UserId = user?.UserId ?? 0 });

                var newConfiguration = _mapper.Map<SettingsRequest, WebHostServiceConfiguration>(request, _config);
                _settingsService.SaveSettingsAs(newConfiguration, nameof(WebHostServiceConfiguration), AppSettingsFilename, true);

                // register new configuration
                _container.RegisterInstance(newConfiguration);

                return Ok(new OperationSuccessResponse());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Settings Error! ", ex));
            }

        }

        /// <summary>
        /// Get the system settings
        /// </summary>
        /// <returns></returns>
        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            try
            {
                var settingsResponse = _mapper.Map<SettingsResponse>(_config);
                return Ok(settingsResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Settings Error! ", ex));
            }

        }

        /// <summary>
        /// Test api settings
        /// </summary>
        /// <returns></returns>
        [HttpPut("/api/settings/testapi")]
        public async Task<IActionResult> TestApiAsync(TestApiRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name)) return BadRequest();

                // test api
                var result = await _integrationService.TestApiAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Settings Error! ", ex));
            }

        }

        /// <summary>
        /// Forget cached credentials
        /// </summary>
        /// <returns></returns>
        [HttpPut("/api/settings/forgetcredentials")]
        public async Task<IActionResult> ForgetCredentialsAsync(ForgetCachedCredentialsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name)) return BadRequest();

                // test api
                var result = await _integrationService.ForgetCachedCredentialsAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Settings Error! ", ex));
            }

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
        /// Perform a complete backup of Binner
        /// </summary>
        /// <returns></returns>
        [HttpPost("backup")]
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
