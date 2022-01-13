using AnyMapper;
using Binner.Common.Models;
using Binner.Common.Services;
using Binner.Model.Common;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class PartTypeController : ControllerBase
    {
        private readonly ILogger<PartTypeController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartTypeService _partTypeService;
        private readonly IPartService _partService;

        public PartTypeController(ILogger<PartTypeController> logger, WebHostServiceConfiguration config, IPartTypeService partTypeService, IPartService partService)
        {
            _logger = logger;
            _config = config;
            _partTypeService = partTypeService;
            _partService = partService;
        }

        /// <summary>
        /// Get a list of part types
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetPartTypesAsync()
        {
            var partTypes = await _partService.GetPartTypesAsync();
            var partTypesResponse = Mapper.Map<ICollection<PartType>, ICollection<PartTypeResponse>>(partTypes);
            foreach (var partType in partTypesResponse)
            {
                var partsForPartType = await _partService.GetPartsAsync(x => x.PartTypeId == partType.PartTypeId);
                partType.Parts = partsForPartType.Count;
            }
            return Ok(partTypesResponse);
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreatePartTypeAsync(CreatePartTypeRequest request)
        {
            var mappedPartType = Mapper.Map<CreatePartTypeRequest, PartType>(request);
            mappedPartType.DateCreatedUtc = DateTime.UtcNow;
            var partType = await _partTypeService.AddPartTypeAsync(mappedPartType);
            return Ok(partType);
        }

        /// <summary>
        /// Update an existing project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdatePartTypeAsync(UpdatePartTypeRequest request)
        {
            var mappedPartType = Mapper.Map<UpdatePartTypeRequest, PartType>(request);
            var partType = await _partTypeService.UpdatePartTypeAsync(mappedPartType);
            return Ok(partType);
        }

        /// <summary>
        /// Delete an existing project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeletePartAsync(DeletePartTypeRequest request)
        {
            var isDeleted = await _partTypeService.DeletePartTypeAsync(new PartType
            {
                PartTypeId = request.PartTypeId
            });
            return Ok(isDeleted);
        }
    }
}
