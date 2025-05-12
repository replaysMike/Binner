using AnyMapper;
using AutoMapper;
using Binner.Common.Services;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.KiCad;
using Binner.Model.Responses;
using Binner.Web.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    /// <summary>
    /// KiCad Http Library support
    /// </summary>
    /// <remarks>https://dev-docs.kicad.org/en/apis-and-binding/http-libraries/index.html</remarks>
    [KiCadTokenAuthorize]
    [Route("kicad-api/v1")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class KiCadController : ControllerBase
    {
        private readonly ILogger<KiCadController> _logger;
        private readonly IRequestContextAccessor _requestContextAccessor;
        private readonly IMapper _mapper;
        private readonly IPartService _partService;

        public KiCadController(ILogger<KiCadController> logger, IMapper mapper, IPartService partService, IRequestContextAccessor requestContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _partService = partService;
            _requestContextAccessor = requestContextAccessor;
        }

        /// <summary>
        /// Endpoint that KiCad will initially connect to, to get the endpoint paths and verify the server is accessible
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation($"[KiCadApi] Hello requested from IP '{_requestContextAccessor.GetIpAddress()}'.");
            return Ok(new
            {
                categories = "", // use default: /kicad-api/v1/categories.json
                parts = "" // use default: /kicad-api/v1/parts/category/{category}.json
            });
        }

        /// <summary>
        /// Get a list of parts for a specified category
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("parts/category/{categoryId}.json")]
        public async Task<IActionResult> GetPartsByCategoryAsync(string categoryId)
        {
            _logger.LogInformation($"[KiCadApi] GET /parts/category/{categoryId}.json from IP '{_requestContextAccessor.GetIpAddress()}'.");
            PartType? partType;
            // parse the part type
            if (int.TryParse(categoryId, out var id))
            {
                partType = await _partService.GetPartTypeAsync(id);
            }
            else
            {
                // it's not an id, its the name
                partType = await _partService.GetPartTypeAsync(categoryId);
            }

            if (partType == null) return NotFound();

            var parts = await _partService.GetPartsByPartTypeAsync(partType);
            if (!parts.Any()) return NotFound();

            var partResponse = _mapper.Map<ICollection<Part>, ICollection<KiCadPart>>(parts);
            _logger.LogInformation($"[KiCadApi] GET /parts/category/{categoryId}.json returned {partResponse.Count} parts from IP '{_requestContextAccessor.GetIpAddress()}'.");
            return Ok(partResponse);
        }

        /// <summary>
        /// Get a list of part details for a specified category
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("parts/{categoryId}.json")]
        public async Task<IActionResult> GetDetailedPartsByCategoryAsync(string categoryId)
        {
            _logger.LogInformation($"[KiCadApi] GET /parts/{categoryId}.json from IP '{_requestContextAccessor.GetIpAddress()}'.");
            PartType? partType;
            // parse the part type
            if (int.TryParse(categoryId, out var id))
            {
                partType = await _partService.GetPartTypeAsync(id);
            }
            else
            {
                // it's not an id, its the name
                partType = await _partService.GetPartTypeAsync(categoryId);
            }

            if (partType == null) return NotFound();

            var parts = await _partService.GetPartsByPartTypeAsync(partType);
            if (!parts.Any()) return NotFound();

            var partDetails = _mapper.Map<ICollection<Part>, ICollection<KiCadPartDetail>>(parts);
            foreach (var partDetail in partDetails)
            {
                var part = parts.Where(x => x.PartId == int.Parse(partDetail.Id)).First();

                partDetail.Fields.Reference = new KiCadValueItem { Value = partType.ReferenceDesignator ?? part.SymbolName?.First().ToString() ?? string.Empty };    // "R"

                if (!string.IsNullOrEmpty(part.Description))
                    partDetail.Fields.Description = new KiCadValueVisibleItem { Value = part.Description };             // "SMD RES 1k 0805"
                
                if (!string.IsNullOrEmpty(part.Value))
                    partDetail.Fields.Value = new KiCadValueItem { Value = part.Value ?? part.PartNumber };                           // "R12"
                
                if (!string.IsNullOrEmpty(part.FootprintName))
                    partDetail.Fields.Footprint = new KiCadValueVisibleItem { Value = part.FootprintName };             // "Resistor_SMD:R_0603_1608Metric"
                if (part.Keywords != null && part.Keywords.Any())
                    partDetail.Fields.Datasheet = new KiCadValueVisibleItem { Value = string.Join(" ", part.Keywords) };// keywords

                // urls
                if (!string.IsNullOrEmpty(part.DatasheetUrl))
                    partDetail.Fields.Datasheet = new KiCadValueVisibleItem { Value = part.DatasheetUrl };
                if (!string.IsNullOrEmpty(part.ProductUrl))
                    partDetail.Fields.Custom1 = new KiCadValueVisibleItem { Value = part.ProductUrl };

                // Custom2 contains additional meta data as key/value pairs
                partDetail.Fields.Custom2 = new KiCadValueVisibleItem { Value = string.Empty };
                var keyValueItems = new List<string>();
                if (!string.IsNullOrEmpty(part.DigiKeyPartNumber))
                    keyValueItems.Add($"DigiKey={part.DigiKeyPartNumber}");
                if (!string.IsNullOrEmpty(part.DigiKeyPartNumber))
                    keyValueItems.Add($"Mouser={part.MouserPartNumber}");
                if (!string.IsNullOrEmpty(part.ArrowPartNumber))
                    keyValueItems.Add($"Arrow={part.DigiKeyPartNumber}");
                if (!string.IsNullOrEmpty(part.DigiKeyPartNumber))
                    keyValueItems.Add($"TME={part.TmePartNumber}");
                if (!string.IsNullOrEmpty(part.ExtensionValue1))
                    keyValueItems.Add($"ExtensionValue1={part.ExtensionValue1}");
                if (!string.IsNullOrEmpty(part.ExtensionValue2))
                    keyValueItems.Add($"ExtensionValue2={part.ExtensionValue2}");
                if (!string.IsNullOrEmpty(part.Location))
                    keyValueItems.Add($"Location={part.Location}");
                if (!string.IsNullOrEmpty(part.BinNumber))
                    keyValueItems.Add($"Bin={part.BinNumber}");
                if (!string.IsNullOrEmpty(part.BinNumber2))
                    keyValueItems.Add($"Bin2={part.BinNumber2}");
                if (keyValueItems.Any())
                    partDetail.Fields.Custom2.Value = string.Join(", ", keyValueItems);

                // Custom3 contains multiple values
                if (!string.IsNullOrEmpty(part.ManufacturerPartNumber) || !string.IsNullOrEmpty(part.Manufacturer))
                {
                    partDetail.Fields.Custom3 = new KiCadValueVisibleItem { Value = string.Empty };
                    var values = new List<string>();
                    if (!string.IsNullOrEmpty(part.ManufacturerPartNumber))
                        values.Add(part.ManufacturerPartNumber);
                    if (!string.IsNullOrEmpty(part.Manufacturer))
                        values.Add(part.Manufacturer);
                    if (values.Any())
                        partDetail.Fields.Custom3.Value = string.Join(" / ", values);
                }
            }
            _logger.LogInformation($"[KiCadApi] GET /parts/{categoryId}.json returned {partDetails.Count} parts from IP '{_requestContextAccessor.GetIpAddress()}'.");
            return Ok(partDetails);
        }

        /// <summary>
        /// Get a list of categories
        /// </summary>
        /// <returns></returns>
        [HttpGet("categories.json")]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            _logger.LogInformation($"[KiCadApi] GET /categories.json from IP '{_requestContextAccessor.GetIpAddress()}'.");
            var partTypes = await _partService.GetPartTypesAsync(true);
            if (partTypes == null) return NotFound();

            var partResponse = _mapper.Map<ICollection<PartType>, ICollection<KiCadCategory>>(partTypes);
            _logger.LogInformation($"[KiCadApi] GET /categories.json returned {partResponse.Count} categories from IP '{_requestContextAccessor.GetIpAddress()}'.");
            return Ok(partResponse);
        }
    }
}
