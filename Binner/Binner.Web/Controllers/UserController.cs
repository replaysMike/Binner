using Binner.Services;
using Binner.Model;
using Binner.Model.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Binner.LicensedProvider;
using System.Security;
using Binner.Web.Conventions;

namespace Binner.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Authorize(Policy = Binner.Model.Authentication.AuthorizationPolicies.Admin)]
    [GenericControllerNameConvention]
    public partial class UserController<TUser> : ControllerBase
        where TUser : User, new()
    {
        private readonly ILogger<UserController<TUser>> _logger;
        private readonly IUserService<TUser> _userService;

        public UserController(ILogger<UserController<TUser>> logger, IUserService<TUser> userService)
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
            try
            {
                var users = await _userService.GetUsersAsync(request);
                return Ok(users);
            }
            catch (LicenseActionException ex)
            {
                return BadRequest(new LicenseResponse(ex));
            }
            catch (LicenseException ex)
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired, new LicenseResponse(ex));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Users api error! ", ex));
            }
        }

        /// <summary>
        /// Get a user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserAsync([FromQuery] TUser request)
        {
            try
            {
                var user = await _userService.GetUserAsync(request);
                if (user == null) return NotFound();
                return Ok(user);
            }
            catch (LicenseActionException ex)
            {
                return BadRequest(new LicenseResponse(ex));
            }
            catch (LicenseException ex)
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired, new LicenseResponse(ex));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Users api error! ", ex));
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync(TUser request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                return Ok(user);
            }
            catch(LicenseActionException ex)
            {
                return BadRequest(new LicenseResponse(ex));
            }
            catch (LicenseException ex)
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired, new LicenseResponse(ex));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Users api error! ", ex));
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateUserAsync(TUser request)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(request);
                return Ok(user);
            }
            catch (LicenseActionException ex)
            {
                return BadRequest(new LicenseResponse(ex));
            }
            catch (LicenseException ex)
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired, new LicenseResponse(ex));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Users api error! ", ex));
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteUserAsync([FromBody] int userId)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId);
                return Ok(new { result });
            }
            catch (SecurityException ex)
            {
                return BadRequest(new LicenseResponse(ex));
            }
            catch (LicenseActionException ex)
            {
                return BadRequest(new LicenseResponse(ex));
            }
            catch (LicenseException ex)
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired, new LicenseResponse(ex));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Users api error! ", ex));
            }
        }
    }
}
