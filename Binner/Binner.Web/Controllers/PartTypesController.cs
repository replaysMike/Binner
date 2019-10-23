using Binner.Common.Services;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class PartTypesController : ControllerBase
    {
        private readonly ILogger<PartTypesController> _logger;
        private readonly IMemoryCache _cache;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;

        public PartTypesController(ILogger<PartTypesController> logger, IMemoryCache cache, WebHostServiceConfiguration config, IPartService partService)
        {
            _logger = logger;
            _cache = cache;
            _config = config;
            _partService = partService;
        }

        /// <summary>
        /// Get a list of part types
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet()]
        public async Task<IActionResult> GetPartTypesAsync()
        {
            var partTypes = await _partService.GetPartTypesAsync();
            return Ok(partTypes);
        }
    }
}
