using AnyMapper;
using Binner.Common.Configuration;
using Binner.Common.IO.Printing;
using Binner.Common.Models;
using Binner.Common.Models.Responses;
using Binner.Common.Services;
using Binner.Model.Common;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.RegularExpressions;
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
        private readonly IPartTypeService _partTypeService;
        private readonly IProjectService _projectService;
        private readonly ILabelPrinterHardware _labelPrinter;
        private readonly IBarcodeGenerator _barcodeGenerator;

        public PartController(ILogger<PartController> logger, WebHostServiceConfiguration config, IPartService partService, IPartTypeService partTypeService, IProjectService projectService, ILabelPrinterHardware labelPrinter, IBarcodeGenerator barcodeGenerator)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _partTypeService = partTypeService;
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
            var partsPage = await _partService.GetPartsAsync(request);
            var partsResponse = Mapper.Map<ICollection<PartResponse>>(partsPage.Items);
            if (partsResponse.Any())
            {
                var partTypes = await _partService.GetPartTypesAsync();
                // map fields that can't be automapped
                foreach (var part in partsResponse)
                {
                    part.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name).FirstOrDefault();
                    part.MountingType = ((MountingType)part.MountingTypeId).ToString();
                    part.Keywords = string.Join(" ", partsPage.Items.First(x => x.PartId == part.PartId).Keywords ?? new List<string>());
                }
            }
            return Ok(new PaginatedResponse<PartResponse>(partsPage.TotalItems, partsPage.PageSize, partsPage.PageNumber, partsResponse));
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

        [HttpGet("barcode/info")]
        public async Task<IActionResult> GetBarcodeInfoAsync([FromQuery] string barcode)
        {
            var partDetails = await _partService.GetBarcodeInfoAsync(barcode);
            return Ok(partDetails);
        }

        /// <summary>
        /// Create multiple new parts
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkPartsAsync(CreateBulkPartRequest request)
        {
            var updatedParts = new List<Part>();
            var mappedParts = Mapper.Map<ICollection<PartBase>, ICollection<Part>>(request.Parts);
            var partTypes = await _partTypeService.GetPartTypesAsync();
            var defaultPartType = partTypes.Where(x => x.Name.Equals("Other", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() ?? partTypes.FirstOrDefault();
            foreach (var mappedPart in mappedParts)
            {
                // does it already exist?
                var existingSearch = await _partService.FindPartsAsync(mappedPart.PartNumber);
                if (existingSearch.Any())
                {
                    // update it's quantity only
                    var existingPart = existingSearch.First().Result;
                    existingPart.Quantity += mappedPart.Quantity;
                    updatedParts.Add(await _partService.UpdatePartAsync(existingPart));
                }
                else
                {
                    var isMapped = false;
                    // if it's numeric only, try getting barcode information
                    var isNumber = new Regex(@"^\d+$");
                    if (isNumber.Match(mappedPart.PartNumber).Success)
                    {
                        var barcodeResult = await _partService.GetBarcodeInfoAsync(mappedPart.PartNumber);
                        if (barcodeResult.Response.Parts.Any())
                        {
                            // convert this entry to a part
                            var entry = barcodeResult.Response.Parts.First();
                            var partType = partTypes.Where(x => x.Name == entry.PartType).FirstOrDefault();
                            mappedPart.PartNumber = entry.ManufacturerPartNumber;
                            mappedPart.PartTypeId = partType != null ? partType.PartTypeId : defaultPartType.PartTypeId;
                            mappedPart.MountingTypeId = entry.MountingTypeId;
                            mappedPart.DatasheetUrl = entry.DatasheetUrls.FirstOrDefault();
                            mappedPart.ManufacturerPartNumber = entry.ManufacturerPartNumber;
                            mappedPart.Manufacturer = entry.Manufacturer;
                            mappedPart.Description = entry.Description;
                            mappedPart.DigiKeyPartNumber = entry.SupplierPartNumber;
                            mappedPart.Keywords = entry.Keywords ?? new List<string>();
                            mappedPart.ImageUrl = entry.ImageUrl;
                            mappedPart.LowStockThreshold = 10;
                            mappedPart.ProductUrl = entry.ProductUrl;
                            mappedPart.PackageType = entry.PackageType;
                            mappedPart.Cost = (decimal)entry.Cost;
                            isMapped = true;
                        }
                    }

                    // if we didn't already map it using above methods, get metadata on the part
                    if (!isMapped)
                    {
                        var metadataResponse = await _partService.GetPartInformationAsync(mappedPart.PartNumber);
                        var digikeyParts = metadataResponse.Response.Parts.Where(x => x.Supplier.Equals("Digikey", StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.DatasheetUrls.Any()).ThenBy(x => x.Cost).ToList();
                        var mouserParts = metadataResponse.Response.Parts.Where(x => x.Supplier.Equals("Mouser", StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.DatasheetUrls.Any()).ThenBy(x => x.Cost).ToList();
                        var metadata = metadataResponse.Response.Parts.FirstOrDefault();
                        if (metadata != null)
                        {
                            var partType = partTypes.Where(x => x.Name == metadata.PartType).FirstOrDefault();
                            mappedPart.PartTypeId = partType != null ? partType.PartTypeId : defaultPartType.PartTypeId;
                            mappedPart.MountingTypeId = metadata.MountingTypeId;
                            mappedPart.Description = metadata.Description;
                            mappedPart.DatasheetUrl = metadata.DatasheetUrls.FirstOrDefault();
                            mappedPart.DigiKeyPartNumber = digikeyParts.FirstOrDefault()?.SupplierPartNumber;
                            mappedPart.MouserPartNumber = mouserParts.FirstOrDefault()?.SupplierPartNumber;
                            mappedPart.ImageUrl = metadata.ImageUrl;
                            mappedPart.Manufacturer = metadata.Manufacturer;
                            mappedPart.ManufacturerPartNumber = metadata.ManufacturerPartNumber;
                            mappedPart.Keywords = metadata.Keywords ?? new List<string>();
                            mappedPart.LowStockThreshold = 10;
                            mappedPart.ProductUrl = metadata.ProductUrl;
                            mappedPart.PackageType = metadata.PackageType;
                            mappedPart.Cost = (decimal)metadata.Cost;
                        }
                        else
                        {
                            // map some default values as we don't know what this is
                            mappedPart.PartTypeId = defaultPartType.PartTypeId;
                            mappedPart.MountingTypeId = (int)MountingType.ThroughHole;
                        }
                    }
                    updatedParts.Add(await _partService.AddPartAsync(mappedPart));
                }
            }

            var partResponse = Mapper.Map<ICollection<Part>, ICollection<PartResponse>>(updatedParts);
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
        public async Task<IActionResult> SearchAsync([FromQuery] string keywords)
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
            var uniquePartsCount = await _partService.GetUniquePartsCountAsync();
            var partsCount = await _partService.GetPartsCountAsync();
            var partsCost = await _partService.GetPartsValueAsync();
            var lowStockCount = await _partService.GetLowStockAsync(new PaginatedRequest { Results = 999 });
            var projectsCount = await _projectService.GetProjectsAsync(new PaginatedRequest { Results = 999 });
            return Ok(new
            {
                UniquePartsCount = uniquePartsCount,
                PartsCount = partsCount,
                PartsCost = partsCost,
                LowStockCount = lowStockCount.Items.Count(),
                ProjectsCount = projectsCount.Count,
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
            var lowStockPage = await _partService.GetLowStockAsync(request);
            var partsResponse = Mapper.Map<ICollection<PartResponse>>(lowStockPage.Items);
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
            return Ok(new PaginatedResponse<PartResponse>(lowStockPage.TotalItems, lowStockPage.PageSize, lowStockPage.PageNumber, partsResponse));
        }

        /// <summary>
        /// Get part information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("info")]
        public async Task<IActionResult> GetPartInfoAsync([FromQuery] string partNumber, [FromQuery] string partTypeId = "", [FromQuery] string mountingTypeId = "")
        {
            var partType = partTypeId;
            var mountingType = mountingTypeId;
            if (int.TryParse(partTypeId, out var _partTypeId))
            {
                var _partType = await _partTypeService.GetPartTypeAsync(_partTypeId);
                if (_partType != null) partType = _partType.Name;
            }
            if (int.TryParse(mountingTypeId, out var _mountingTypeId))
            {
                if (Enum.IsDefined(typeof(MountingType), _mountingTypeId))
                {
                    var _mountingType = (MountingType)_mountingTypeId;
                    mountingType = _mountingType.ToString();
                }
            }

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
                    existingPart.Quantity += commonPart.QuantityAvailable;
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
            try
            {
                var part = await _partService.GetPartAsync(request.PartNumber);
                if (part == null) return NotFound();
                var stream = new MemoryStream();
                var image = _labelPrinter.PrintLabel(new LabelContent { Part = part }, new PrinterOptions(request.GenerateImageOnly));
                image.SaveAsPng(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(stream, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Print Error! ", ex));
            }
        }

        /// <summary>
        /// Preview part label
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("preview")]
        public async Task<IActionResult> PreviewPrintPartAsync([FromQuery] PrintPartRequest request)
        {
            try
            {
                var part = await _partService.GetPartAsync(request.PartNumber);
                Image<Rgba32> image;
                if (part == null)
                {
                    // generate a general barcode as the part isn't created or doesn't exist
                    image = _barcodeGenerator.GenerateBarcode(request.PartNumber, Color.Black, Color.White, 300, 25);
                }
                else
                {
                    // generate a label for a part
                    image = _labelPrinter.PrintLabel(new LabelContent { Part = part }, new PrinterOptions(true));
                }
                var stream = new MemoryStream();
                image.SaveAsPng(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(stream, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Print Error! ", ex));
            }
        }

        /// <summary>
        /// Generate a part barcode
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("barcode")]
        public IActionResult BarcodePart([FromQuery] GetPartRequest request)
        {
            try
            {
                var stream = new MemoryStream();
                var image = _barcodeGenerator.GenerateBarcode(request.PartNumber, Color.Black, Color.White, 300, 25);
                image.SaveAsPng(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(stream, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Barcode Error! ", ex));
            }
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
