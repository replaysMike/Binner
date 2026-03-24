using Binner.Data;
using Binner.LicensedProvider;
using Binner.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mime;
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
        private readonly ILicensedService<User, BinnerContext> _licensedService;

        public LicenseController(ILogger<LicenseController> logger, IAuthenticationService<BinnerContext> authenticationService, ILicensedService<User, BinnerContext> licensedService)
        {
            _logger = logger;
            _authenticationService = authenticationService;
            _licensedService = licensedService;
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
            // validate the license on the licensing server. This will return the license information and limitations if valid, or an error if invalid or expired
            // note that the license key is validated on each request to licensed api endpoints, so bypassing this call won't do anything as it's used to auto renew licenses.
            return Ok(await _licensedService.ValidateLicenseAsync(licenseKey, deviceId));
        }
    }
}
