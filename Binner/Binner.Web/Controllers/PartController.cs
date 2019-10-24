using AnyMapper;
using Binner.Common.Models;
using Binner.Common.Services;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            var partResponse = Mapper.Map<Part, PartResponse>(part);
            var partTypes = await _partService.GetPartTypesAsync();
            partResponse.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name).FirstOrDefault();
            return Ok(partResponse);
        }

        /// <summary>
        /// Get an existing part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetPartsAsync([FromQuery] PaginatedRequest request)
        {
            var parts = await _partService.GetPartsAsync(request);
            var partsResponse = Mapper.Map<ICollection<Part>, ICollection<PartResponse>>(parts);
            if (partsResponse.Any())
            {
                var partTypes = await _partService.GetPartTypesAsync();
                // map part types
                foreach (var part in partsResponse)
                    part.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name).FirstOrDefault();
            }
            return Ok(partsResponse);
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
            var partResponse = Mapper.Map<Part, PartResponse>(part);
            partResponse.PartType = partType.Name;
            return Ok(partResponse);
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
            mappedPart.PartId = request.PartId;
            mappedPart.Keywords = request.Keywords?.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            mappedPart.PartTypeId = partType.PartTypeId;
            var part = await _partService.UpdatePartAsync(mappedPart);
            var partResponse = Mapper.Map<Part, PartResponse>(part);
            partResponse.PartType = partType.Name;
            return Ok(partResponse);
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
            var partTypes = await _partService.GetPartTypesAsync();
            var partsOrdered = parts.OrderBy(x => x.Rank).Select(x => x.Result).ToList();
            var partsResponse = Mapper.Map<ICollection<Part>, ICollection<PartResponse>>(partsOrdered);
            // map part types
            foreach (var part in partsResponse)
                part.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name).FirstOrDefault();

            return Ok(partsResponse);
        }

        /// <summary>
        /// Get part information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("info")]
        public async Task<IActionResult> GetPartInfoAsync([FromQuery]string partNumber, [FromQuery]string partType = "", [FromQuery]string mountingType = "")
        {
            var metadata = await _partService.GetPartInformationAsync(partNumber, partType, mountingType);
            if (metadata == null)
                return NotFound();
            return Ok(metadata);
        }

        /// <summary>
        /// Get part metadata
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
