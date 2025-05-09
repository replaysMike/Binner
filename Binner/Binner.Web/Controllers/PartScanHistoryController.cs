﻿using AnyMapper;
using Binner.Common.IO;
using Binner.Common.Services;
using Binner.Model;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class PartScanHistoryController : ControllerBase
    {
        private readonly ILogger<PartScanHistoryController> _logger;
        private readonly IPartScanHistoryService _partScanHistoryService;

        public PartScanHistoryController(ILogger<PartScanHistoryController> logger, IPartScanHistoryService partScanHistoryService)
        {
            _logger = logger;
            _partScanHistoryService = partScanHistoryService;
        }

        /// <summary>
        /// Get an existing part scan history
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]GetPartScanHistoryRequest request)
        {
            return await SearchAsync(request);
        }

        /// <summary>
        /// Get an existing part scan history
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("search")]
        public async Task<IActionResult> SearchAsync(GetPartScanHistoryRequest request)
        {
            PartScanHistory? partScanHistory = null;
            if (request.PartScanHistoryId > 0)
            {
                partScanHistory = await _partScanHistoryService.GetPartScanHistoryAsync(request.PartScanHistoryId);
            }
            if (!string.IsNullOrEmpty(request.RawScan))
            {
                if (request.SearchCrc)
                    // search using the raw scan string
                    partScanHistory = await _partScanHistoryService.GetPartScanHistoryAsync(request.RawScan);
                else
                    // search using the crc of the raw scan string
                    partScanHistory = await _partScanHistoryService.GetPartScanHistoryAsync(Checksum.Compute(request.RawScan));
            }
            else if (request.Crc > 0)
            {
                // search using provided crc
                partScanHistory = await _partScanHistoryService.GetPartScanHistoryAsync(request.Crc);
            }

            if (partScanHistory == null) return NotFound();

            return Ok(Mapper.Map<PartScanHistory, PartScanHistoryResponse>(partScanHistory));
        }

        /// <summary>
        /// Create a new part scan history
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreatePartScanHistoryAsync(CreatePartScanHistoryRequest request)
        {
            var mappedPartScanHistory = Mapper.Map<CreatePartScanHistoryRequest, PartScanHistory>(request);
            mappedPartScanHistory.DateCreatedUtc = DateTime.UtcNow;
            var project = await _partScanHistoryService.AddPartScanHistoryAsync(mappedPartScanHistory);
            return Ok(project);
        }

        /// <summary>
        /// Update an existing part scan history
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdatePartScanHistoryAsync(UpdatePartScanHistoryRequest request)
        {
            var mappedPartScanHistory = Mapper.Map<UpdatePartScanHistoryRequest, PartScanHistory>(request);
            var project = await _partScanHistoryService.UpdatePartScanHistoryAsync(mappedPartScanHistory);
            return Ok(project);
        }

        /// <summary>
        /// Delete an existing part scan history
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeletePartScanHistoryAsync(DeletePartScanHistoryRequest request)
        {
            var isDeleted = await _partScanHistoryService.DeletePartScanHistoryAsync(new PartScanHistory
            {
                PartScanHistoryId = request.PartScanHistoryId
            });
            return Ok(isDeleted);
        }
    }
}
