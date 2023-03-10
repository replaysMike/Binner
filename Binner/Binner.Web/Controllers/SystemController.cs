using Binner.Common.Configuration;
using Binner.Common.IO.Printing;
using Binner.Common.Models.Requests;
using Binner.Common.Models.Responses;
using Binner.Common.Services;
using LightInject;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("[controller]")]
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

        public SystemController(AutoMapper.IMapper mapper, IServiceContainer container, ILogger<ProjectController> logger, WebHostServiceConfiguration config, ISettingsService settingsService, IntegrationService integrationService, ILabelPrinterHardware labelPrinter, FontManager fontManager)
        {
            _mapper = mapper;
            _container = container;
            _logger = logger;
            _config = config;
            _settingsService = settingsService;
            _integrationService = integrationService;
            _labelPrinter = labelPrinter;
            _fontManager = fontManager;
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
                if (string.IsNullOrEmpty(request.Octopart.ApiUrl)) request.Octopart.ApiUrl = "https://octopart.com";
                request.Binner.ApiUrl = $"https://{request.Binner.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Digikey.ApiUrl = $"https://{request.Digikey.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Digikey.oAuthPostbackUrl = $"https://{request.Digikey.oAuthPostbackUrl.Replace("https://", "").Replace("http://", "")}";
                request.Mouser.ApiUrl = $"https://{request.Mouser.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Octopart.ApiUrl = $"https://{request.Octopart.ApiUrl.Replace("https://", "").Replace("http://", "")}";

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
        [HttpPut("/settings/testapi")]
        public async Task<IActionResult> TestApiAsync(TestApiRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name)) return BadRequest();

                // test api
                var result = await _integrationService.TestApiAsync(request.Name);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Settings Error! ", ex));
            }

        }
    }
}
