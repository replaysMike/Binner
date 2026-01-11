using AutoMapper;
using Binner.Global.Common;
using Binner.Model.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly ILogger<LicenseController> _logger;
        private readonly IRequestContextAccessor _requestContextAccessor;
        private readonly IMapper _mapper;

        public LicenseController(ILogger<LicenseController> logger, IMapper mapper, IRequestContextAccessor requestContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _requestContextAccessor = requestContextAccessor;
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateLicenseAsync([FromQuery] string licenseKey, [FromQuery] string? deviceId)
        {
            var uri = new Uri($"https://binner.io");
            var httpClient = new HttpClient()
            {
                BaseAddress = uri
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
            return StatusCode((int)response.StatusCode);
        }
    }
}
