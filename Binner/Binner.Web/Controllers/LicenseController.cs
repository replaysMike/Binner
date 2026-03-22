using AutoMapper;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Responses;
using Binner.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class LicenseController : ControllerBase
    {
        private readonly TimeSpan LicenseValidationTimeout = TimeSpan.FromSeconds(30);
        private readonly ILogger<LicenseController> _logger;
        private readonly IAuthenticationService<BinnerContext> _authenticationService;

        public LicenseController(ILogger<LicenseController> logger, IAuthenticationService<BinnerContext> authenticationService)
        {
            _logger = logger;
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Get the current user's license key
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetLicenseKeysAsync()
        {
            var licenseKeys = new List<LicenseKey>();
            // license key will be blank if user is not admin
            var licenseKey = await _authenticationService.ValidateLicenseAsync();
            licenseKeys.Add(licenseKey);
            return Ok(licenseKeys);
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateLicenseAsync([FromQuery] string licenseKey, [FromQuery] string? deviceId)
        {
            var errorMessage = string.Empty;
            try
            {
#if DEBUG
                var uri = new Uri($"https://localhost:9090");
#else
                var uri = new Uri($"https://binner.io");
#endif
                var httpClient = new HttpClient()
                {
                    BaseAddress = uri,
                    Timeout = LicenseValidationTimeout
                };
                var response = await httpClient.GetAsync($"api/license/validate?licenseKey={licenseKey}&deviceId={deviceId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    var result = JsonSerializer.Deserialize<ValidateLicenseKeyResponse>(json, options);
                    _logger.LogInformation($"[Validate license key] IsValid: {result?.IsValidated} Message: {result?.Message}");
                    return Ok(result);
                }
                return Ok(new ValidateLicenseKeyResponse
                {
                    IsValidated = false,
                    Message = $"Received status code {response.StatusCode} from validation server."
                });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, $"Failed to validate license due to timeout!");
                errorMessage = $"Failed to validate license due to timeout!";
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, $"Failed to validate license due to timeout!");
                errorMessage = $"Failed to validate license due to timeout!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to validate license due to exception!");
                errorMessage = $"Failed to validate license due to exception! Exception: {ex.GetBaseException().Message}";
            }
            return Ok(new ValidateLicenseKeyResponse
            {
                IsValidated = false,
                Message = errorMessage
            });
        }
    }
}
