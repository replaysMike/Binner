using Binner.Common.Integrations;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Binner.Services;
using LightInject;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly ISettingsService _settingsService;
        private readonly IntegrationService _integrationService;
        private readonly AutoMapper.IMapper _mapper;
        private readonly IUserConfigurationService _userConfigurationService;

        public SettingsController(AutoMapper.IMapper mapper, ILogger<SettingsController> logger, WebHostServiceConfiguration config, ISettingsService settingsService, IntegrationService integrationService, IUserConfigurationService userConfigurationService)
        {
            _mapper = mapper;
            _logger = logger;
            _config = config;
            _settingsService = settingsService;
            _integrationService = integrationService;
            _userConfigurationService = userConfigurationService;
        }

        /// <summary>
        /// Save the system settings
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = Binner.Model.Authentication.AuthorizationPolicies.Admin)]
        public async Task<IActionResult> SaveSettingsAsync(SettingsRequest request) 
        {
            try
            {
                // validate formats
                if (string.IsNullOrEmpty(request.Binner.ApiUrl)) request.Binner.ApiUrl = "https://swarm.binner.io";
                if (string.IsNullOrEmpty(request.Digikey.ApiUrl)) request.Digikey.ApiUrl = "https://api.digikey.com";
                if (string.IsNullOrEmpty(request.Digikey.oAuthPostbackUrl)) request.Digikey.oAuthPostbackUrl = "https://localhost:8090/Authorization/Authorize";
                if (string.IsNullOrEmpty(request.Mouser.ApiUrl)) request.Mouser.ApiUrl = "https://api.mouser.com";
                if (string.IsNullOrEmpty(request.Arrow.ApiUrl)) request.Arrow.ApiUrl = "https://api.arrow.com";
                if (string.IsNullOrEmpty(request.Tme.ApiUrl)) request.Tme.ApiUrl = "https://api.tme.eu";
                request.Binner.ApiUrl = $"https://{request.Binner.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Digikey.ApiUrl = $"https://{request.Digikey.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Digikey.oAuthPostbackUrl = $"https://{request.Digikey.oAuthPostbackUrl.Replace("https://", "").Replace("http://", "")}";
                request.Mouser.ApiUrl = $"https://{request.Mouser.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Arrow.ApiUrl = $"https://{request.Arrow.ApiUrl.Replace("https://", "").Replace("http://", "")}";
                request.Tme.ApiUrl = $"https://{request.Tme.ApiUrl.Replace("https://", "").Replace("http://", "")}";

                var integrationConfiguration = _mapper.Map<UserIntegrationConfiguration>(request);
                var printerConfiguration = _mapper.Map<UserPrinterConfiguration>(request);
                var localeConfiguration = _mapper.Map<UserLocaleConfiguration>(request);
                var barcodeConfiguration = _mapper.Map<UserBarcodeConfiguration>(request);
                integrationConfiguration = await _userConfigurationService.CreateOrUpdateIntegrationConfigurationAsync(integrationConfiguration);
                printerConfiguration = await _userConfigurationService.CreateOrUpdatePrinterConfigurationAsync(printerConfiguration);
                localeConfiguration = await _userConfigurationService.CreateOrUpdateLocaleConfigurationAsync(localeConfiguration);
                barcodeConfiguration = await _userConfigurationService.CreateOrUpdateBarcodeConfigurationAsync(barcodeConfiguration);

                // also save the custom fields (add/update/remove)
                await _settingsService.SaveCustomFieldsAsync(request.CustomFields);

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
        [HttpGet]
        public async Task<IActionResult> GetSettingsAsync()
        {
            try
            {
                var settingsResponse = _mapper.Map<SettingsResponse>(_config);
                settingsResponse.CustomFields = await _settingsService.GetCustomFieldsAsync();
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
        [HttpPut("testapi")]
        [Authorize(Policy = Binner.Model.Authentication.AuthorizationPolicies.Admin)]
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
        [HttpPut("forgetcredentials")]
        [Authorize(Policy = Binner.Model.Authentication.AuthorizationPolicies.Admin)]
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
    }
}
