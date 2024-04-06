using AnyMapper;
using Binner.Common;
using Binner.Common.IO.Printing;
using Binner.Common.Services;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing;
using Binner.Model.IO.Printing.PrinterHardware;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
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
        private readonly IUserService _userService;
        private readonly IPrintService _printService;
        private readonly ILabelGenerator _labelGenerator;

        public PartController(ILogger<PartController> logger, WebHostServiceConfiguration config, IPartService partService, IPartTypeService partTypeService, IProjectService projectService, ILabelPrinterHardware labelPrinter, IBarcodeGenerator barcodeGenerator, IUserService userService, IPrintService printService, ILabelGenerator labelGenerator)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _partTypeService = partTypeService;
            _projectService = projectService;
            _labelPrinter = labelPrinter;
            _barcodeGenerator = barcodeGenerator;
            _userService = userService;
            _printService = printService;
            _labelGenerator = labelGenerator;
        }

        /// <summary>
        /// Get an existing part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetPartRequest request)
        {
            if (string.IsNullOrEmpty(request.PartNumber))
                return BadRequest($"No part number specified!");
            var response = await _partService.GetPartWithStoredFilesAsync(request);
            if (response.Part == null) return NotFound();
            var partResponse = Mapper.Map<Part, PartStoredFilesResponse>(response.Part);
            partResponse.StoredFiles = response.StoredFiles;
            var partTypes = await _partService.GetPartTypesAsync();
            partResponse.PartType = partTypes.Where(x => x.PartTypeId == response.Part.PartTypeId).Select(x => x.Name).FirstOrDefault();
            partResponse.Keywords = string.Join(" ", response.Part.Keywords ?? new List<string>());
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
            if (string.IsNullOrEmpty(request.PartTypeId))
                return BadRequest($"No part type specified!");

            var mappedPart = Mapper.Map<CreatePartRequest, Part>(request);
            mappedPart.Keywords = request.Keywords?.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);

            var partType = await GetPartTypeAsync(request.PartTypeId);
            if (partType == null) return BadRequest($"Invalid Part Type: {request.PartTypeId}");

            mappedPart.PartTypeId = partType.PartTypeId;
            mappedPart.MountingTypeId = GetMountingTypeId(request.MountingTypeId ?? string.Empty);

            if (mappedPart.MountingTypeId < 0) return BadRequest($"Invalid Mounting Type: {request.MountingTypeId}");

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
            if (string.IsNullOrEmpty(request.PartTypeId))
                return BadRequest($"No part type specified!");

            var mappedPart = Mapper.Map<UpdatePartRequest, Part>(request);
            var partType = await GetPartTypeAsync(request.PartTypeId);
            if (partType == null) return BadRequest($"Invalid Part Type: {request.PartTypeId}");

            mappedPart.PartTypeId = partType.PartTypeId;
            mappedPart.MountingTypeId = GetMountingTypeId(request.MountingTypeId ?? string.Empty);

            if (mappedPart.MountingTypeId < 0) return BadRequest($"Invalid Mounting Type: {request.MountingTypeId}");

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
            var partDetails = await _partService.GetBarcodeInfoAsync(barcode, ScannedBarcodeType.Product);
            return Ok(partDetails);
        }

        [HttpGet("barcode/packlist/info")]
        public async Task<IActionResult> GetPacklistBarcodeInfoAsync([FromQuery] string barcode)
        {
            var partDetails = await _partService.GetBarcodeInfoAsync(barcode, ScannedBarcodeType.Packlist);
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
            var partsRequested = request.Parts ?? new List<PartBase>();
            var mappedParts = Mapper.Map<ICollection<PartBase>, ICollection<Part>>(partsRequested);
            var partTypes = await _partTypeService.GetPartTypesAsync();
            var defaultPartType = partTypes
                .FirstOrDefault(x => x.Name?.Equals("Other", StringComparison.InvariantCultureIgnoreCase) == true) ?? partTypes.First();
            foreach (var mappedPart in mappedParts)
            {
                // does it already exist?
                if (string.IsNullOrEmpty(mappedPart.PartNumber))
                    continue;
                var existingSearch = await _partService.FindPartsAsync(mappedPart.PartNumber);
                if (existingSearch.Any())
                {
                    // update it's basic data only
                    var existingPart = existingSearch.First().Result;
                    existingPart.Quantity = mappedPart.Quantity;
                    existingPart.Description = mappedPart.Description;
                    existingPart.Location = mappedPart.Location;
                    existingPart.BinNumber = mappedPart.BinNumber;
                    existingPart.BinNumber2 = mappedPart.BinNumber2;
                    updatedParts.Add(await _partService.UpdatePartAsync(existingPart));
                }
                else
                {
                    var isMapped = false;
                    var barcode = string.Empty;
                    var partRequested = partsRequested?.Where(x => x.PartNumber == mappedPart.PartNumber).FirstOrDefault();
                    if (partRequested != null && !string.IsNullOrEmpty(partRequested.Barcode))
                        barcode = partRequested.Barcode;
                    else
                    {
                        // if it's numeric only, try getting barcode information
                        var isNumber = Regex.Match(@"^\d+$", mappedPart.PartNumber).Success;
                        if (isNumber) barcode = mappedPart.PartNumber;

                    }

                    if (!string.IsNullOrEmpty(barcode))
                    {
                        var barcodeResult = await _partService.GetBarcodeInfoAsync(barcode, ScannedBarcodeType.Product);
                        if (barcodeResult.Response?.Parts.Any() == true)
                        {
                            // convert this entry to a part
                            var entry = barcodeResult.Response.Parts.First();
                            var partType = partTypes
                                .FirstOrDefault(x => x.Name == entry.PartType) ?? partTypes.First();
                            mappedPart.PartNumber = entry.ManufacturerPartNumber;
                            mappedPart.PartTypeId = partType.PartTypeId;
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
                            mappedPart.Cost = entry.Cost;
                            isMapped = true;
                        }
                    }

                    // if we didn't already map it using above methods, get metadata on the part
                    if (!isMapped)
                    {
                        if (string.IsNullOrEmpty(mappedPart.PartNumber))
                            continue;
                        var metadataResponse = await _partService.GetPartInformationAsync(mappedPart.PartNumber);
                        if (metadataResponse.Response != null)
                        {
                            var digikeyParts = metadataResponse.Response.Parts
                                .Where(x =>
                                    x.Supplier?.Equals("Digikey", StringComparison.InvariantCultureIgnoreCase) == true)
                                .OrderByDescending(x => x.DatasheetUrls.Any())
                                .ThenBy(x => x.Cost)
                                .ToList();
                            var mouserParts = metadataResponse.Response.Parts
                                .Where(x => x.Supplier?.Equals("Mouser", StringComparison.InvariantCultureIgnoreCase) ==
                                            true)
                                .OrderByDescending(x => x.DatasheetUrls.Any())
                                .ThenBy(x => x.Cost)
                                .ToList();
                            var arrowParts = metadataResponse.Response.Parts.Where(x => x.Supplier.Equals("Arrow", StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.DatasheetUrls.Any()).ThenBy(x => x.Cost).ToList();
                            var metadata = metadataResponse.Response.Parts.FirstOrDefault();
                            if (metadata != null)
                            {
                                var partType = partTypes.FirstOrDefault(x => x.Name == metadata.PartType) ??
                                               defaultPartType;
                                mappedPart.PartTypeId = partType.PartTypeId;
                                mappedPart.MountingTypeId = metadata.MountingTypeId;
                                mappedPart.Description = metadata.Description;
                                mappedPart.DatasheetUrl = metadata.DatasheetUrls.FirstOrDefault();
                                mappedPart.DigiKeyPartNumber = digikeyParts.FirstOrDefault()?.SupplierPartNumber;
                                mappedPart.MouserPartNumber = mouserParts.FirstOrDefault()?.SupplierPartNumber;
                                mappedPart.ArrowPartNumber = arrowParts.FirstOrDefault()?.SupplierPartNumber;
                                mappedPart.ImageUrl = metadata.ImageUrl;
                                mappedPart.Manufacturer = metadata.Manufacturer;
                                mappedPart.ManufacturerPartNumber = metadata.ManufacturerPartNumber;
                                mappedPart.Keywords = metadata.Keywords ?? new List<string>();
                                mappedPart.LowStockThreshold = 10;
                                mappedPart.ProductUrl = metadata.ProductUrl;
                                mappedPart.PackageType = metadata.PackageType;
                                mappedPart.Cost = metadata.Cost;
                            }
                            else
                            {
                                // map some default values as we don't know what this is
                                mappedPart.PartTypeId = defaultPartType.PartTypeId;
                                mappedPart.MountingTypeId = (int)MountingType.None;
                            }
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
        /// <param name="exactMatch">True if only searching for an exact match</param>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync([FromQuery] string keywords, [FromQuery] bool exactMatch = false)
        {
            if (exactMatch)
            {
                // search by exact part name match
                var part = await _partService.GetPartAsync(new GetPartRequest { PartNumber = keywords });
                if (part == null) return NotFound();

                var partTypes = await _partService.GetPartTypesAsync();
                var mappedPart = Mapper.Map<Part, PartResponse>(part);
                mappedPart.PartType = partTypes
                    .Where(x => x.PartTypeId == mappedPart.PartTypeId)
                    .Select(x => x.Name)
                    .FirstOrDefault();
                mappedPart.MountingType = ((MountingType)mappedPart.MountingTypeId).ToString();
                var keywordsList = part.Keywords;
                if (keywordsList != null)
                    mappedPart.Keywords = string.Join(" ", keywordsList);
                var partResponse = new List<PartResponse>
                {
                    mappedPart
                };

                return Ok(partResponse);
            }
            else
            {
                // search by keyword
                var parts = await _partService.FindPartsAsync(keywords);
                if (!parts.Any())
                    return NotFound();
                var partTypes = await _partService.GetPartTypesAsync();
                var partsOrdered = parts
                    .OrderBy(x => x.Rank)
                    .Select(x => x.Result)
                    .ToList();
                var partsResponse = Mapper.Map<ICollection<Part>, ICollection<PartResponse>>(partsOrdered);
                // map part types
                foreach (var mappedPart in partsResponse)
                {
                    mappedPart.PartType = partTypes
                        .Where(x => x.PartTypeId == mappedPart.PartTypeId)
                        .Select(x => x.Name)
                        .FirstOrDefault();
                    mappedPart.MountingType = ((MountingType)mappedPart.MountingTypeId).ToString();
                    var keywordsList = partsOrdered.First(x => x.PartId == mappedPart.PartId).Keywords;
                    if (keywordsList != null)
                        mappedPart.Keywords = string.Join(" ", keywordsList);
                }

                return Ok(partsResponse);
            }
        }

        /// <summary>
        /// Get part summary (dashboard)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummaryAsync()
        {
            try
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
                    Currency = _config.Locale.Currency.ToString().ToUpper()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Error", ex));
            }
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
        /// <param name="partNumber"></param>
        /// <param name="partTypeId"></param>
        /// <param name="mountingTypeId"></param>
        /// <param name="supplierPartNumbers">List of supplier part numbers if known, in the format: 'suppliername:partnumber,suppliername2:partnumber'</param>
        /// <returns></returns>
        [HttpGet("info")]
        public async Task<IActionResult> GetPartInfoAsync([FromQuery] string partNumber, [FromQuery] string partTypeId = "", [FromQuery] string mountingTypeId = "", [FromQuery] string supplierPartNumbers = "")
        {
            try
            {
                var partType = partTypeId;
                var mountingType = mountingTypeId;
                if (int.TryParse(partTypeId, out var parsedPartTypeId))
                {
                    var partTypeWithName = await _partTypeService.GetPartTypeAsync(parsedPartTypeId);
                    if (partTypeWithName != null) partType = partTypeWithName.Name;
                }
                if (int.TryParse(mountingTypeId, out var parsedMountingTypeId))
                {
                    if (Enum.IsDefined(typeof(MountingType), parsedMountingTypeId))
                    {
                        var mountingTypeEnum = (MountingType)parsedMountingTypeId;
                        mountingType = mountingTypeEnum.ToString();
                    }
                }

                var metadata = await _partService.GetPartInformationAsync(partNumber, partType ?? string.Empty, mountingType, supplierPartNumbers);
                return Ok(metadata);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Failed to fetch part information! ", ex));
            }
        }

        /// <summary>
        /// External order import search
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("import")]
        public async Task<IActionResult> OrderImportAsync(OrderImportRequest request)
        {
            if (string.IsNullOrEmpty(request.OrderId)) return BadRequest("No OrderId specified");
            if (string.IsNullOrEmpty(request.Supplier)) return BadRequest("No Supplier specified");
            try
            {
                var metadata = await _partService.GetExternalOrderAsync(request);
                return Ok(metadata);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Failed to import order! ", ex));
            }
        }

        /// <summary>
        /// External order import parts
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("importparts")]
        public async Task<IActionResult> OrderImportPartsAsync(OrderImportPartsRequest request)
        {
            var response = new OrderImportResponse
            {
                OrderId = request.OrderId,
                Supplier = request.Supplier
            };
            if (request.Parts == null || request.Parts.Count == 0)
                return Ok(response);

            foreach (var commonPart in request.Parts)
            {
                try
                {
                    var existingParts = await _partService.GetPartsAsync(x => x.ManufacturerPartNumber == commonPart.ManufacturerPartNumber);
                    if (existingParts.Any())
                    {
                        var existingPart = existingParts.First();
                        // update quantity and cost
                        var existingQuantity = existingPart.Quantity;
                        existingPart.Quantity += commonPart.QuantityAvailable;
                        existingPart.Cost = commonPart.Cost;
                        existingPart.Currency = commonPart.Currency;
                        existingPart = await _partService.UpdatePartAsync(existingPart);
                        var successPart = Mapper.Map<CommonPart, ImportPartResponse>(commonPart);
                        if (string.IsNullOrEmpty(commonPart.PartType))
                        {
                            successPart.PartType = SystemDefaults.DefaultPartTypes.Other.ToString();
                        }
                        successPart.QuantityExisting = existingQuantity;
                        successPart.QuantityAdded = commonPart.QuantityAvailable;
                        successPart.IsImported = true;
                        response.Parts.Add(successPart);
                    }
                    else
                    {
                        // create new part
                        var part = Mapper.Map<CommonPart, Part>(commonPart);
                        part.Quantity += commonPart.QuantityAvailable;
                        part.Cost = commonPart.Cost;
                        part.Currency = commonPart.Currency;
                        if (commonPart.Supplier?.Equals("digikey", StringComparison.InvariantCultureIgnoreCase) == true)
                            part.DigiKeyPartNumber = commonPart.SupplierPartNumber;
                        if (commonPart.Supplier?.Equals("mouser", StringComparison.InvariantCultureIgnoreCase) == true)
                            part.MouserPartNumber = commonPart.SupplierPartNumber;
                        if (commonPart.Supplier?.Equals("arrow", StringComparison.InvariantCultureIgnoreCase) == true)
                            part.ArrowPartNumber = commonPart.SupplierPartNumber;
                        part.DatasheetUrl = commonPart.DatasheetUrls.FirstOrDefault();
                        part.PartNumber = commonPart.ManufacturerPartNumber;

                        PartType? partType = null;
                        if (!string.IsNullOrEmpty(commonPart.PartType))
                        {
                            partType = await GetPartTypeAsync(commonPart.PartType);
                            part.PartTypeId = partType?.PartTypeId ?? (int)SystemDefaults.DefaultPartTypes.Other;
                        }
                        else if (part.PartTypeId == 0)
                        {
                            part.PartTypeId = (int)SystemDefaults.DefaultPartTypes.Other;
                        }

                        part.DateCreatedUtc = DateTime.UtcNow;
                        part = await _partService.AddPartAsync(part);
                        var successPart = Mapper.Map<CommonPart, ImportPartResponse>(commonPart);
                        successPart.PartType = partType?.Name ?? SystemDefaults.DefaultPartTypes.Other.ToString();

                        successPart.QuantityExisting = 0;
                        successPart.QuantityAdded = commonPart.QuantityAvailable;
                        successPart.IsImported = true;
                        response.Parts.Add(successPart);
                    }
                }
                catch (Exception ex)
                {
                    var failedPart = Mapper.Map<CommonPart, ImportPartResponse>(commonPart);
                    failedPart.IsImported = false;
                    failedPart.ErrorMessage = ex.GetBaseException().Message;
                    response.Parts.Add(failedPart);
                }
            }

            return Ok(response);
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
                if (string.IsNullOrEmpty(request.PartNumber)) return BadRequest("No part number specified.");
                var part = await _partService.GetPartAsync(new GetPartRequest { PartNumber = request.PartNumber, PartId = request.PartId });
                if (part == null) return NotFound();

                if (await _printService.HasPartLabelTemplateAsync())
                {
                    // use the new part label template
                    var label = await _printService.GetPartLabelTemplateAsync();
                    var image = _labelGenerator.CreateLabelImage(label, part);
                    var stream = new MemoryStream();
                    await image.SaveAsPngAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    // load the label template
                    var template = await _printService.GetLabelTemplateAsync(label.LabelTemplateId);

                    if (!request.GenerateImageOnly)
                        _labelPrinter.PrintLabelImage(image, new PrinterOptions((LabelSource)(template?.LabelPaperSource ?? 0), template.Name, false));

                    return new FileStreamResult(stream, "image/png");
                }
                else
                {

                    var stream = new MemoryStream();
                    var image = _labelPrinter.PrintLabel(new LabelContent { Part = part }, new PrinterOptions(request.GenerateImageOnly));
                    await image.SaveAsPngAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return new FileStreamResult(stream, "image/png");
                }
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
        [AllowAnonymous]
        [HttpGet("preview")]
        public async Task<IActionResult> PreviewPrintPartAsync([FromQuery] PrintPartRequest request)
        {
            try
            {
                var userContext = await _userService.ValidateUserImageToken(request.Token ?? string.Empty);
                if (userContext == null) return GetInvalidTokenImage();
                System.Threading.Thread.CurrentPrincipal = new TokenPrincipal(userContext, request.Token);

                if (string.IsNullOrEmpty(request.PartNumber)) return BadRequest("No part number specified.");
                var part = await _partService.GetPartAsync(new GetPartRequest { PartNumber = request.PartNumber, PartId = request.PartId });

                var stream = new MemoryStream();
                if (await _printService.HasPartLabelTemplateAsync())
                {
                    // use the new part label template
                    var label = await _printService.GetPartLabelTemplateAsync();
                    var image = _labelGenerator.CreateLabelImage(label, part ?? new Part { PartNumber = request.PartNumber });
                    await image.SaveAsPngAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return new FileStreamResult(stream, "image/png");
                }
                else
                {
                    // generate a label for a part

                    // generate a general barcode as the part isn't created or doesn't exist
                    Image<Rgba32> image;
                    if (part == null)
                        image = _barcodeGenerator.GenerateBarcode(request.PartNumber, Color.Black, Color.White, 300, 25);
                    else
                        image = _labelPrinter.PrintLabel(new LabelContent { Part = part }, new PrinterOptions(true));
                    await image.SaveAsPngAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return new FileStreamResult(stream, "image/png");
                }
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
        [AllowAnonymous]
        [HttpGet("barcode")]
        public async Task<IActionResult> BarcodePart([FromQuery] GetPartImageRequest request)
        {
            try
            {
                var userContext = await _userService.ValidateUserImageToken(request.Token ?? string.Empty);
                if (userContext == null) return GetInvalidTokenImage();
                System.Threading.Thread.CurrentPrincipal = new TokenPrincipal(userContext, request.Token);

                if (string.IsNullOrEmpty(request.PartNumber))
                    return BadRequest("No part number specified.");
                var stream = new MemoryStream();
                var image = _barcodeGenerator.GenerateBarcode(request.PartNumber, Color.Black, Color.White, 300, 25);
                await image.SaveAsPngAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(stream, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Barcode Error! ", ex));
            }
        }

        private FileStreamResult GetInvalidTokenImage()
        {
            var image = new BlankImage(300, 100, Color.White, Color.Red, "Invalid Image Token!\nYou may need to re-login.");
            var stream = new MemoryStream();
            image.Image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "image/png");
        }

        /// <summary>
        /// Create a part supplier
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("partSupplier")]
        public async Task<IActionResult> CreatePartSupplierAsync(CreatePartSupplierRequest request)
        {
            if (request.PartId <= 0)
                return BadRequest("No partId specified.");
            return Ok(await _partService.AddPartSupplierAsync(Mapper.Map<PartSupplier>(request)));
        }

        /// <summary>
        /// Update a part supplier
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("partSupplier")]
        public async Task<IActionResult> UpdatePartSupplierAsync(UpdatePartSupplierRequest request)
        {
            if (request.PartId <= 0)
                return BadRequest("No partId specified.");
            return Ok(await _partService.UpdatePartSupplierAsync(Mapper.Map<PartSupplier>(request)));
        }

        /// <summary>
        /// Delete a part supplier
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("partSupplier")]
        public async Task<IActionResult> DeletePartSupplierAsync(DeletePartSupplierRequest request)
        {
            if (request.PartSupplierId <= 0)
                return BadRequest("No partSupplierId specified.");
            return Ok(await _partService.DeletePartSupplierAsync(new PartSupplier { PartSupplierId = request.PartSupplierId }));
        }

        /// <summary>
        /// Check for duplicate part
        /// </summary>
        /// <param name="request"></param>
        /// <param name="part"></param>
        /// <returns></returns>
        private async Task<IActionResult?> CheckForDuplicateAsync(CreatePartRequest request, Part part)
        {
            if (request is IPreventDuplicateResource && !((IPreventDuplicateResource)request).AllowPotentialDuplicate)
            {
                if (string.IsNullOrEmpty(part.PartNumber))
                    return BadRequest("No part number specified.");
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
        private async Task<PartType?> GetPartTypeAsync(string partType)
        {
            PartType? result = null;
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
