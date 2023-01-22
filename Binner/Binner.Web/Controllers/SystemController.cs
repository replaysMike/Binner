using Binner.Common.Configuration;
using Binner.Common.IO.Printing;
using Binner.Common.Models.Requests;
using Binner.Common.Models.Responses;
using Binner.Common.Services;
using Binner.Web.Configuration;
using LightInject;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;

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
        private readonly ILabelPrinterHardware _labelPrinter;
        private readonly FontManager _fontManager;
        private readonly AutoMapper.IMapper _mapper;
        private readonly IServiceContainer _container;

        public SystemController(AutoMapper.IMapper mapper, IServiceContainer container, ILogger<ProjectController> logger, WebHostServiceConfiguration config, ISettingsService settingsService, ILabelPrinterHardware labelPrinter, FontManager fontManager)
        {
            _mapper = mapper;
            _container = container;
            _logger = logger;
            _config = config;
            _settingsService = settingsService;
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
    }
}
