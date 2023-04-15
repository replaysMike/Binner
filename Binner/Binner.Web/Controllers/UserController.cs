using Binner.Common.Models;
using Binner.Model.Configuration;
using Binner.Common.Models.Requests;
using Binner.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Binner.Model.Common;

namespace Binner.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Authorize(Policy = Binner.Model.Authentication.AuthorizationPolicies.Admin)]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Get list of users
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetUsersAsync([FromQuery] PaginatedRequest request)
        {
            var users = await _userService.GetUsersAsync(request);
            return Ok(users);
        }

        /// <summary>
        /// Get a user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserAsync([FromQuery] User request)
        {
            var user = await _userService.GetUserAsync(request);
            return Ok(user);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync(User request)
        {
            var user = await _userService.CreateUserAsync(request);
            return Ok(user);
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateUserAsync(User request)
        {
            var user = await _userService.UpdateUserAsync(request);
            return Ok(user);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteUserAsync([FromBody] int userId)
        {
            var result = await _userService.DeleteUserAsync(userId);
            return Ok(new { result });
        }
    }
}
