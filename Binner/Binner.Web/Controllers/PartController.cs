using AnyMapper;
using Binner.Common.IO.Printing;
using Binner.Common.Models;
using Binner.Common.Models.Responses;
using Binner.Common.Services;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
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
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;
        private readonly IProjectService _projectService;
        private readonly ILabelPrinter _labelPrinter;
        private readonly IBarcodeGenerator _barcodeGenerator;

        public PartController(ILogger<PartController> logger, WebHostServiceConfiguration config, IPartService partService, IProjectService projectService, ILabelPrinter labelPrinter, IBarcodeGenerator barcodeGenerator)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _projectService = projectService;
            _labelPrinter = labelPrinter;
            _barcodeGenerator = barcodeGenerator;
        }

        /// <summary>
        /// Get an existing part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetPartRequest request)
        {
            var part = await _partService.GetPartAsync(request.PartNumber);
            if (part == null) return NotFound();
            var partResponse = Mapper.Map<Part, PartResponse>(part);
            var partTypes = await _partService.GetPartTypesAsync();
            partResponse.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name).FirstOrDefault();
            partResponse.Keywords = string.Join(" ", part.Keywords ?? new List<string>());
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
                // map fields that can't be automapped
                foreach (var part in partsResponse)
                {
                    part.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name).FirstOrDefault();
                    part.MountingType = ((MountingType)part.MountingTypeId).ToString();
                    part.Keywords = string.Join(" ", parts.First(x => x.PartId == part.PartId).Keywords);
                }
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
            var mappedPart = Mapper.Map<CreatePartRequest, Part>(request);
            mappedPart.Keywords = request.Keywords?.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);

            var partType = await GetPartTypeAsync(request.PartTypeId);
            if (partType == null) return BadRequest($"Invalid Part Type: {request.PartTypeId}");
            mappedPart.PartTypeId = partType.PartTypeId;
            mappedPart.MountingTypeId = GetMountingTypeId(request.MountingTypeId);
            if (mappedPart.MountingTypeId <= 0) return BadRequest($"Invalid Mounting Type: {request.MountingTypeId}");

            var duplicatePartResponse = await CheckForDuplicateAsync(request, mappedPart);
            if (duplicatePartResponse != null)
                return duplicatePartResponse;
            
            var part = await _partService.AddPartAsync(mappedPart);
            var partResponse = Mapper.Map<Part, PartResponse>(part);
            partResponse.PartType = partType.Name;
            partResponse.Keywords = string.Join(" ", part.Keywords ?? new List<string>());
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
            var mappedPart = Mapper.Map<UpdatePartRequest, Part>(request);
            var partType = await GetPartTypeAsync(request.PartTypeId);
            if (partType == null) return BadRequest($"Invalid Part Type: {request.PartTypeId}");
            mappedPart.PartTypeId = partType.PartTypeId;
            mappedPart.MountingTypeId = GetMountingTypeId(request.MountingTypeId);
            if (mappedPart.MountingTypeId <= 0) return BadRequest($"Invalid Mounting Type: {request.MountingTypeId}");

            mappedPart.PartId = request.PartId;
            mappedPart.Keywords = request.Keywords?.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
            var part = await _partService.UpdatePartAsync(mappedPart);
            var partResponse = Mapper.Map<Part, PartResponse>(part);
            partResponse.PartType = partType.Name;
            partResponse.Keywords = string.Join(" ", part.Keywords ?? new List<string>());
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
            {
                part.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name).FirstOrDefault();
                part.MountingType = ((MountingType)part.MountingTypeId).ToString();
                part.Keywords = string.Join(" ", partsOrdered.First(x => x.PartId == part.PartId).Keywords);
            }

            return Ok(partsResponse);
        }

        /// <summary>
        /// Get part summary (dashboard)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummaryAsync()
        {
            var count = await _partService.GetPartsCountAsync();
            var lowStock = await _partService.GetLowStockAsync(new PaginatedRequest { Results = 999 });
            var projects = await _projectService.GetProjectsAsync(new PaginatedRequest { Results = 999 });
            return Ok(new
            {
                PartsCount = count,
                LowStockCount = lowStock.Count,
                ProjectsCount = projects.Count,
            });
        }

        /// <summary>
        /// Get list of low stock
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("low")]
        public async Task<IActionResult> GetLowStockAsync([FromQuery] PaginatedRequest request)
        {
            var lowStock = await _partService.GetLowStockAsync(request);
            var partsResponse = Mapper.Map<ICollection<Part>, ICollection<PartResponse>>(lowStock);
            if (partsResponse.Any())
            {
                var partTypes = await _partService.GetPartTypesAsync();
                // map part types
                foreach (var part in partsResponse)
                {
                    part.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name).FirstOrDefault();
                    part.MountingType = ((MountingType)part.MountingTypeId).ToString();
                    part.Keywords = partsResponse.First(x => x.PartId == part.PartId).Keywords;
                }
            }
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
        /// External order import search
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("import")]
        public async Task<IActionResult> OrderImportAsync(OrderImportRequest request)
        {
            var metadata = await _partService.GetExternalOrderAsync(request.OrderId, request.Supplier);
            if (metadata == null)
                return NotFound();
            return Ok(metadata);
        }

        /// <summary>
        /// External order import parts
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("importparts")]
        public async Task<IActionResult> OrderImportPartsAsync(OrderImportPartsRequest request)
        {
            var parts = new List<PartResponse>();
            foreach (var commonPart in request.Parts)
            {
                var existingParts = await _partService.GetPartsAsync(x => x.ManufacturerPartNumber == commonPart.ManufacturerPartNumber);
                if (existingParts.Any())
                {
                    var existingPart = existingParts.First();
                    // update quantity
                    existingPart.Quantity += commonPart.Quantity;
                    existingPart = await _partService.UpdatePartAsync(existingPart);
                    parts.Add(Mapper.Map<Part, PartResponse>(existingPart));
                }
                else
                {
                    // create new part
                    var part = Mapper.Map<CommonPart, Part>(commonPart);
                    if (commonPart.Supplier.Equals("digikey", StringComparison.InvariantCultureIgnoreCase))
                        part.DigiKeyPartNumber = commonPart.SupplierPartNumber;
                    if (commonPart.Supplier.Equals("mouser", StringComparison.InvariantCultureIgnoreCase))
                        part.MouserPartNumber = commonPart.SupplierPartNumber;
                    part.DatasheetUrl = commonPart.DatasheetUrls.FirstOrDefault();
                    part.PartNumber = commonPart.ManufacturerPartNumber;
                    part.DateCreatedUtc = DateTime.UtcNow;
                    part = await _partService.AddPartAsync(part);
                    var mappedPart = Mapper.Map<Part, PartResponse>(part);
                    mappedPart.Keywords = string.Join(" ", part.Keywords ?? new List<string>());
                    parts.Add(mappedPart);
                }
            }

            return Ok(parts);
        }

        /// <summary>
        /// Print a part label
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("print")]
        public async Task<IActionResult> PrintPartAsync([FromQuery] PrintPartRequest request)
        {
            var part = await _partService.GetPartAsync(request.PartNumber);
            if (part == null) return NotFound();
            var stream = new MemoryStream();
            var image = _labelPrinter.PrintLabel(new LabelContent { Part = part }, new PrinterOptions(request.GenerateImageOnly));
            image.Save(stream, ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "image/png");
        }

        /// <summary>
        /// Generate a part barcode
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("barcode")]
        public IActionResult BarcodePart([FromQuery] GetPartRequest request)
        {
            var stream = new MemoryStream();
            var image = _barcodeGenerator.GenerateBarcode(request.PartNumber, 300, 25);
            image.Save(stream, ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "image/png");
        }

        /// <summary>
        /// Check for duplicate part
        /// </summary>
        /// <param name="request"></param>
        /// <param name="part"></param>
        /// <returns></returns>
        private async Task<IActionResult> CheckForDuplicateAsync(CreatePartRequest request, Part part)
        {
            if (request is IPreventDuplicateResource && !((IPreventDuplicateResource)request).AllowPotentialDuplicate)
            {
                var existingSearch = await _partService.FindPartsAsync(part.PartNumber);
                if (existingSearch.Any())
                {
                    var existingParts = existingSearch.Select(x => x.Result).ToList();
                    var partsResponse = Mapper.Map<ICollection<Part>, ICollection<PartResponse>>(existingParts);
                    var partTypes = await _partService.GetPartTypesAsync();
                    foreach (var p in partsResponse)
                    {
                        p.PartType = partTypes.Where(x => x.PartTypeId == p.PartTypeId).Select(x => x.Name).FirstOrDefault();
                        p.MountingType = ((MountingType)p.MountingTypeId).ToString();
                    }
                    return StatusCode((int)HttpStatusCode.Conflict, new PossibleDuplicateResponse(partsResponse));
                }
            }
            return null;
        }

        /// <summary>
        /// Get an existing part type, or create if it doesn't exist
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        private async Task<PartType> GetPartTypeAsync(string partType)
        {
            PartType result = null;
            if (int.TryParse(partType, out int partTypeId))
            {
                // numeric format
                result = await _partService.GetPartTypeAsync(partTypeId);
            }
            else
            {
                // string format
                result = await _partService.GetOrCreatePartTypeAsync(new PartType
                {
                    Name = partType
                });
            }
            return result;
        }

        private int GetMountingTypeId(string mountingType)
        {
            if (int.TryParse(mountingType, out int mountingTypeId))
            {
                // numeric format
                if (Enum.IsDefined(typeof(MountingType), mountingTypeId))
                    return mountingTypeId;
            }
            else
            {
                // string format
                if (Enum.IsDefined(typeof(MountingType), mountingType))
                    return (int)Enum.Parse<MountingType>(mountingType.Replace(" ", ""), true);
            }
            return -1;
        }
    }
}
