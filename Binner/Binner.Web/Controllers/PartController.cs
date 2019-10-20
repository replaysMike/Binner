using AnyMapper;
using Binner.Common.Models;
using Binner.Common.Services;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class PartController : ControllerBase
    {
        private readonly ILogger<PartController> _logger;
        private readonly IMemoryCache _cache;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;

        public PartController(ILogger<PartController> logger, IMemoryCache cache, WebHostServiceConfiguration config, IPartService partService)
        {
            _logger = logger;
            _cache = cache;
            _config = config;
            _partService = partService;
        }

        /// <summary>
        /// Get an existing part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(GetPartRequest request)
        {
            var part = await _partService.GetPartAsync(request.PartNumber);
            if (part == null) return NotFound();

            return Ok(part);
        }

        /// <summary>
        /// Get an existing part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetPartsAsync([FromQuery] PaginatedRequest request)
        {
            return Ok(await _partService.GetPartsAsync(request));
        }

        /// <summary>
        /// Create a new part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreatePartAsync(CreatePartRequest request)
        {
            var partType = await _partService.GetOrCreatePartTypeAsync(new PartType
            {
                Name = request.PartType
            });
            var mappedPart = Mapper.Map<CreatePartRequest, Part>(request);
            mappedPart.Keywords = request.Keywords?.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            mappedPart.PartTypeId = partType.PartTypeId;
            var part = await _partService.AddPartAsync(mappedPart);
            return Ok(part);
        }

        /// <summary>
        /// Update an existing part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdatePartAsync(UpdatePartRequest request)
        {
            var partType = await _partService.GetOrCreatePartTypeAsync(new PartType
            {
                Name = request.PartType
            });
            var mappedPart = Mapper.Map<UpdatePartRequest, Part>(request);
            mappedPart.Keywords = request.Keywords?.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            mappedPart.PartTypeId = partType.PartTypeId;
            var part = await _partService.UpdatePartAsync(mappedPart);
            return Ok(part);
        }

        /// <summary>
        /// Delete an existing part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeletePartAsync(DeletePartRequest request)
        {
            var isDeleted = await _partService.DeletePartAsync(new Part
            {
                PartId = request.PartId
            });
            return Ok(isDeleted);
        }

        /// <summary>
        /// Search parts by keyword(s)
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync([FromQuery]string keywords)
        {
            var parts = await _partService.FindPartsAsync(keywords);
            if (parts == null || !parts.Any())
                return NotFound();
            return Ok(parts.OrderBy(x => x.Rank).Select(x => x.Result));
        }

        /// <summary>
        /// Delete an existing part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("metadata")]
        public async Task<IActionResult> GetMetadataAsync([FromQuery]string partNumber)
        {
            var metadata = await _partService.GetPartMetadataAsync(partNumber);
            if (metadata == null)
                return NotFound();
            return Ok(metadata);
        }
    }
}
