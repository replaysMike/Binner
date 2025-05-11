using AnyMapper;
using Binner.Common;
using Binner.Common.Extensions;
using Binner.Common.IO;
using Binner.Common.IO.Printing;
using Binner.Common.Services;
using Binner.Model;
using Binner.Model.Barcode;
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
using static Binner.Model.Common.SystemDefaults;

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
        private readonly IPartScanHistoryService _partScanHistoryService;
        private readonly IOrderImportHistoryService _orderImportHistoryService;
        private readonly ILabelPrinterHardware _labelPrinter;
        private readonly IBarcodeGenerator _barcodeGenerator;
        private readonly IUserService _userService;
        private readonly IPrintService _printService;
        private readonly ILabelGenerator _labelGenerator;

        public PartController(ILogger<PartController> logger, WebHostServiceConfiguration config, IPartService partService, IPartTypeService partTypeService, IProjectService projectService, IPartScanHistoryService partScanHistoryService, IOrderImportHistoryService orderImportHistoryService, ILabelPrinterHardware labelPrinter, IBarcodeGenerator barcodeGenerator, IUserService userService, IPrintService printService, ILabelGenerator labelGenerator)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _partTypeService = partTypeService;
            _projectService = projectService;
            _partScanHistoryService = partScanHistoryService;
            _orderImportHistoryService = orderImportHistoryService;
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
            if (string.IsNullOrEmpty(request.PartNumber))
                return BadRequest($"No part number specified!");

            var mappedPart = Mapper.Map<CreatePartRequest, Part>(request);
            mappedPart.Keywords = request.Keywords?.Split([" ", ","], StringSplitOptions.RemoveEmptyEntries);

            var (partType, errorMessage) = await ValidatePartTypeAsync(request.PartTypeId, request.PartNumber);
            if (partType == null)
                return BadRequest(errorMessage);

            mappedPart.PartTypeId = partType.PartTypeId;
            EnsureValidMountingType(mappedPart, request.MountingTypeId);

            var duplicatePartResponse = await CheckForDuplicateAsync(request, mappedPart);
            if (duplicatePartResponse != null)
                return duplicatePartResponse;

            var part = await _partService.AddPartAsync(mappedPart);

            if (request.BarcodeObject != null)
            {
                // add the scanned barcode to history
                await AddScanHistoryAsync(part, request.BarcodeObject);
            }

            var partResponse = Mapper.Map<Part, PartResponse>(part);
            partResponse.PartType = partType.Name;
            partResponse.Keywords = string.Join(" ", part.Keywords ?? new List<string>());
            return Ok(partResponse);
        }

        private async Task<(PartType? PartType, string? ErrorMessage)> ValidatePartTypeAsync(string? partTypeId, string? partNumber)
        {
            var partType = await GetPartTypeAsync(partTypeId);

            if (partType == null)
            {
                // new behavior: default to a partType and log the warning
                _logger.LogWarning($"Unknown part type '{partTypeId}' when creating part '{partNumber}'. Defaulting to {nameof(DefaultPartTypes.Other)}");
                partType = await GetPartTypeAsync(DefaultPartTypes.Other);
                if (partType == null)
                {
                    var error = $"Unknown default part type '{DefaultPartTypes.Other}' when creating part '{partNumber}'.";
                    _logger.LogError(error);
                    return (null, error);
                }
            }
            return (partType, null);
        }

        private bool EnsureValidMountingType(Part part, string? mountingTypeId)
        {
            part.MountingTypeId = GetMountingTypeId(mountingTypeId);
            return true;
        }

        private async Task AddScanHistoryAsync(Part part, BarcodeScan b)
        {
            var partScanHistory = new PartScanHistory
            {
                PartId = part.PartId,
                RawScan = b.CorrectedValue ?? b.RawValue,
                BarcodeType = BarcodeTypesHelper.GetBarcodeType(b.Type),
                CountryOfOrigin = b.Value.GetValue("countryOfOrigin").As<string?>(),
                Crc = Checksum.Compute(b.CorrectedValue ?? b.RawValue),
                Description = b.Value.GetValue("description").As<string?>(),
                Invoice = b.Value.GetValue("invoice").As<string?>(),
                LotCode = b.Value.GetValue("lotCode").As<string?>(),
                ManufacturerPartNumber = b.Value.GetValue("mfgPartNumber").As<string?>(),
                Mid = b.Value.GetValue("mid").As<string?>(),
                Packlist = b.Value.GetValue("unknown").As<string?>(),
                Quantity = b.Value.GetValue("quantity").As<int>(),
                SalesOrder = b.Value.GetValue("salesOrder").As<string?>(),
                ScannedLabelType = b.ScannedLabelType,
                Supplier = b.Supplier,
                SupplierPartNumber = b.Value.GetValue("supplierPartNumber").As<string?>(),
            };
            await _partScanHistoryService.AddPartScanHistoryAsync(partScanHistory);
        }

        /// <summary>
        /// Update an existing part
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdatePartAsync(UpdatePartRequest request)
        {
            if (string.IsNullOrEmpty(request.PartNumber))
                return BadRequest($"No part number specified!");

            var mappedPart = Mapper.Map<UpdatePartRequest, Part>(request);
            var (partType, errorMessage) = await ValidatePartTypeAsync(request.PartTypeId, request.PartNumber);
            if (partType == null)
                return BadRequest(errorMessage);

            mappedPart.PartTypeId = partType.PartTypeId;
            EnsureValidMountingType(mappedPart, request.MountingTypeId);

            mappedPart.PartId = request.PartId;
            mappedPart.Keywords = request.Keywords?.Split([" ", ","], StringSplitOptions.RemoveEmptyEntries);
            var part = await _partService.UpdatePartAsync(mappedPart);

            if (request.BarcodeObject != null)
            {
                // add the scanned barcode to history
                await AddScanHistoryAsync(part, request.BarcodeObject);
            }

            var partResponse = Mapper.Map<Part, PartResponse>(part);
            partResponse.PartType = partType.Name;
            partResponse.Keywords = string.Join(" ", part.Keywords ?? new List<string>());
            return Ok(partResponse);
        }

        [HttpGet("barcode/info")]
        public async Task<IActionResult> GetBarcodeInfoAsync([FromQuery] string barcode)
        {
            var partDetails = await _partService.GetBarcodeInfoAsync(barcode, ScannedLabelType.Product);
            return Ok(partDetails);
        }

        [HttpGet("barcode/packlist/info")]
        public async Task<IActionResult> GetPacklistBarcodeInfoAsync([FromQuery] string barcode)
        {
            var partDetails = await _partService.GetBarcodeInfoAsync(barcode, ScannedLabelType.Packlist);
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
            var addedParts = new List<Part>();
            var updatedParts = new List<Part>();
            var partsRequested = request.Parts ?? new List<BulkPart>();
            var mappedParts = Mapper.Map<ICollection<BulkPart>, ICollection<Part>>(partsRequested);
            var partTypes = await _partTypeService.GetPartTypesAsync();
            var defaultPartType = partTypes
                .FirstOrDefault(x => x.Name?.Equals("Other", StringComparison.InvariantCultureIgnoreCase) == true) ?? partTypes.First();
            foreach (var bulkPart in partsRequested)
            {
                // does it already exist?
                if (string.IsNullOrEmpty(bulkPart.PartNumber))
                    continue;
                var mappedPart = Mapper.Map<BulkPart, Part>(bulkPart);
                var existingSearch = await _partService.FindPartsAsync(bulkPart.PartNumber);
                if (existingSearch.Any())
                {
                    // update it's basic data only
                    var existingPart = existingSearch.First().Result;
                    existingPart.Quantity = bulkPart.Quantity;
                    existingPart.Description = bulkPart.Description;
                    existingPart.Location = bulkPart.Location;
                    existingPart.BinNumber = bulkPart.BinNumber;
                    existingPart.BinNumber2 = bulkPart.BinNumber2;
                    if (!string.IsNullOrEmpty(bulkPart.SupplierPartNumber) && string.IsNullOrEmpty(existingPart.DigiKeyPartNumber))
                        existingPart.DigiKeyPartNumber = bulkPart.SupplierPartNumber;
                    if (string.IsNullOrEmpty(existingPart.DigiKeyPartNumber))
                        existingPart.DigiKeyPartNumber = bulkPart.DigiKeyPartNumber;
                    if (string.IsNullOrEmpty(existingPart.MouserPartNumber))
                        existingPart.MouserPartNumber = bulkPart.MouserPartNumber;
                    if (string.IsNullOrEmpty(existingPart.ArrowPartNumber))
                        existingPart.ArrowPartNumber = mappedPart.ArrowPartNumber;
                    if (string.IsNullOrEmpty(existingPart.TmePartNumber))
                        existingPart.TmePartNumber = mappedPart.TmePartNumber;
                    updatedParts.Add(await _partService.UpdatePartAsync(existingPart));
                }
                else
                {
                    // it's a new part
                    var isMapped = false;
                    var searchBarcode = string.Empty;
                    var partRequested = partsRequested?.Where(x => x.PartNumber == mappedPart.PartNumber).FirstOrDefault();
                    var barcode = partRequested?.Barcode;
                    if (partRequested != null && !string.IsNullOrEmpty(barcode))
                        searchBarcode = barcode;
                    else
                    {
                        // if it's numeric only, try getting barcode information
                        var isNumber = Regex.Match(@"^\d+$", mappedPart.PartNumber).Success;
                        if (isNumber) searchBarcode = mappedPart.PartNumber;
                    }

                    if (!string.IsNullOrEmpty(searchBarcode))
                    {
                        var barcodeResult = await _partService.GetBarcodeInfoAsync(searchBarcode, ScannedLabelType.Product);
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
                            if (!string.IsNullOrEmpty(bulkPart.SupplierPartNumber))
                                mappedPart.DigiKeyPartNumber = bulkPart.SupplierPartNumber;
                            if (!string.IsNullOrEmpty(entry.SupplierPartNumber))
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
                    var newPart = await _partService.AddPartAsync(mappedPart);
                    addedParts.Add(newPart);

                    if (bulkPart.BarcodeObject != null)
                    {
                        // add the scanned barcode to history
                        await AddScanHistoryAsync(newPart, bulkPart.BarcodeObject);
                    }

                }
            }

            return Ok(new BulkPartResponse
            {
                Added = Mapper.Map<ICollection<Part>, ICollection<PartResponse>>(addedParts),
                Updated = Mapper.Map<ICollection<Part>, ICollection<PartResponse>>(updatedParts)
            });
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
        /// External order import parts into local inventory
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("importparts")]
        public async Task<IActionResult> OrderImportPartsAsync(OrderImportPartsRequest request)
        {
            var response = new OrderImportResponse
            {
                OrderId = request.OrderId,
                Invoice = request.Invoice,
                Packlist = request.Packlist,
                Supplier = request.Supplier
            };
            if (request.Parts == null || request.Parts.Count == 0)
                return Ok(response);

            // create an order import history record
            var history = await _orderImportHistoryService.AddOrderImportHistoryAsync(new OrderImportHistory
            {
                SalesOrder = request.OrderId,
                Supplier = request.Supplier,
                Invoice = request.Invoice,
                Packlist = request.Packlist,
            });

            foreach (var importedPart in request.Parts)
            {
                try
                {
                    var existingParts = await _partService.GetPartsAsync(x => x.ManufacturerPartNumber == importedPart.ManufacturerPartNumber);
                    if (existingParts.Any())
                    {
                        var existingPart = existingParts.First();
                        // update quantity and cost, as well as fields with no value
                        var existingQuantity = existingPart.Quantity;
                        existingPart.Quantity += importedPart.QuantityAvailable;
                        existingPart.Cost = importedPart.Cost;
                        existingPart.Currency = importedPart.Currency;

                        // fields that may have no current value
                        if (string.IsNullOrEmpty(existingPart.Description))
                            existingPart.Description = importedPart.Description;
                        if (string.IsNullOrEmpty(existingPart.ManufacturerPartNumber))
                            existingPart.ManufacturerPartNumber = importedPart.ManufacturerPartNumber;
                        if (string.IsNullOrEmpty(existingPart.DigiKeyPartNumber) && importedPart.Supplier?.Equals("digikey", StringComparison.InvariantCultureIgnoreCase) == true)
                            existingPart.DigiKeyPartNumber = importedPart.SupplierPartNumber;
                        if (string.IsNullOrEmpty(existingPart.MouserPartNumber) && importedPart.Supplier?.Equals("mouser", StringComparison.InvariantCultureIgnoreCase) == true)
                            existingPart.MouserPartNumber = importedPart.SupplierPartNumber;
                        if (string.IsNullOrEmpty(existingPart.ArrowPartNumber) && importedPart.Supplier?.Equals("arrow", StringComparison.InvariantCultureIgnoreCase) == true)
                            existingPart.ArrowPartNumber = importedPart.SupplierPartNumber;
                        if (string.IsNullOrEmpty(existingPart.TmePartNumber) && importedPart.Supplier?.Equals("tme", StringComparison.InvariantCultureIgnoreCase) == true)
                            existingPart.TmePartNumber = importedPart.SupplierPartNumber;
                        if (string.IsNullOrEmpty(existingPart.DatasheetUrl))
                            existingPart.DatasheetUrl = importedPart.DatasheetUrls.FirstOrDefault();
                        if (string.IsNullOrEmpty(existingPart.ProductUrl))
                            existingPart.ProductUrl = importedPart.ProductUrl;
                        if (string.IsNullOrEmpty(existingPart.Manufacturer))
                            existingPart.Manufacturer = importedPart.Manufacturer;
                        // add the reference line as an extension value
                        if (string.IsNullOrEmpty(existingPart.ExtensionValue1) || existingPart.ExtensionValue1 == importedPart.Reference)
                            existingPart.ExtensionValue1 = importedPart.Reference;
                        else if (string.IsNullOrEmpty(existingPart.ExtensionValue2))
                            existingPart.ExtensionValue2 = importedPart.Reference;

                        existingPart = await _partService.UpdatePartAsync(existingPart);
                        var successPart = Mapper.Map<CommonPart, ImportPartResponse>(importedPart);
                        if (string.IsNullOrEmpty(importedPart.PartType))
                        {
                            successPart.PartType = SystemDefaults.DefaultPartTypes.Other.ToString();
                        }
                        successPart.QuantityExisting = existingQuantity;
                        successPart.QuantityAdded = importedPart.QuantityAvailable;
                        successPart.IsImported = true;
                        response.Parts.Add(successPart);
                    }
                    else
                    {
                        // create new part
                        var part = Mapper.Map<CommonPart, Part>(importedPart);
                        part.Quantity += importedPart.QuantityAvailable;
                        part.Cost = importedPart.Cost;
                        part.Currency = importedPart.Currency;
                        if (importedPart.Supplier?.Equals("digikey", StringComparison.InvariantCultureIgnoreCase) == true)
                            part.DigiKeyPartNumber = importedPart.SupplierPartNumber;
                        if (importedPart.Supplier?.Equals("mouser", StringComparison.InvariantCultureIgnoreCase) == true)
                            part.MouserPartNumber = importedPart.SupplierPartNumber;
                        if (importedPart.Supplier?.Equals("arrow", StringComparison.InvariantCultureIgnoreCase) == true)
                            part.ArrowPartNumber = importedPart.SupplierPartNumber;
                        if (importedPart.Supplier?.Equals("tme", StringComparison.InvariantCultureIgnoreCase) == true)
                            part.TmePartNumber = importedPart.SupplierPartNumber;
                        part.DatasheetUrl = importedPart.DatasheetUrls.FirstOrDefault();
                        part.ProductUrl = importedPart.ProductUrl;
                        part.Manufacturer = importedPart.Manufacturer;
                        part.PartNumber = importedPart.ManufacturerPartNumber;
                        // add the reference line as an extension value
                        part.ExtensionValue1 = importedPart.Reference;

                        PartType? partType = null;
                        if (!string.IsNullOrEmpty(importedPart.PartType))
                        {
                            partType = await GetPartTypeAsync(importedPart.PartType);
                            part.PartTypeId = partType?.PartTypeId ?? (int)SystemDefaults.DefaultPartTypes.Other;
                        }
                        else if (part.PartTypeId == 0)
                        {
                            part.PartTypeId = (int)SystemDefaults.DefaultPartTypes.Other;
                        }

                        part.DateCreatedUtc = DateTime.UtcNow;
                        part = await _partService.AddPartAsync(part);
                        if (part != null)
                        {
                            // create an order import history line item
                            if (history != null)
                            {
                                var historyLineItem = await _orderImportHistoryService.AddOrderImportHistoryLineItemAsync(new OrderImportHistoryLineItem
                                {
                                    OrderImportHistoryId = history.OrderImportHistoryId,
                                    Supplier = importedPart.Supplier,
                                    Cost = importedPart.Cost,
                                    CustomerReference = importedPart.Reference,
                                    Description = importedPart.Description,
                                    Manufacturer = importedPart.Manufacturer,
                                    ManufacturerPartNumber = importedPart.ManufacturerPartNumber,
                                    PartNumber = part.PartNumber,
                                    Quantity = importedPart.QuantityAvailable,
                                    PartId = part.PartId,
                                });
                            }

                            var successPart = Mapper.Map<CommonPart, ImportPartResponse>(importedPart);
                            successPart.PartType = partType?.Name ?? SystemDefaults.DefaultPartTypes.Other.ToString();

                            successPart.QuantityExisting = 0;
                            successPart.QuantityAdded = importedPart.QuantityAvailable;
                            successPart.IsImported = true;
                            response.Parts.Add(successPart);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var failedPart = Mapper.Map<CommonPart, ImportPartResponse>(importedPart);
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
        /// Get list of categories (DigiKey only)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGetCategories()
        {
            var categories = await _partService.GetCategoriesAsync();
            return Ok(categories);
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
        private async Task<PartType?> GetPartTypeAsync(string? partType)
        {
            if (string.IsNullOrEmpty(partType)) return null;

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

        /// <summary>
        /// Get an existing part type from default part types
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        private async Task<PartType?> GetPartTypeAsync(DefaultPartTypes partType)
        {
            return await _partService.GetPartTypeAsync(partType.ToString());
        }

        private int GetMountingTypeId(string? mountingType)
        {
            if (string.IsNullOrEmpty(mountingType))
                return (int)MountingType.None;

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
            return (int)MountingType.None;
        }
    }
}
