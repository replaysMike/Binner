using AutoMapper;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.KiCad;
using Binner.Model.Requests;
using Binner.Services;
using Binner.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using TypeSupport.Extensions;

namespace Binner.Web.Controllers
{
    /// <summary>
    /// KiCad Http Library support
    /// See <seealso cref="KiCadTokenAuthorizationHandler"/> for authentication requirements details.
    /// </summary>
    /// <remarks>https://dev-docs.kicad.org/en/apis-and-binding/http-libraries/index.html</remarks>
    [KiCadTokenAuthorize] // validated by <seealso cref="KiCadTokenAuthorizationHandler"/>
    [Route("kicad-api/v1")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class KiCadController : ControllerBase
    {
        private readonly ILogger<KiCadController> _logger;
        private readonly IRequestContextAccessor _requestContextAccessor;
        private readonly IMapper _mapper;
        private readonly IPartService _partService;
        private readonly IUserConfigurationService _userConfigurationService;

        public KiCadController(ILogger<KiCadController> logger, IMapper mapper, IPartService partService, IRequestContextAccessor requestContextAccessor, IUserConfigurationService userConfigurationService)
        {
            _logger = logger;
            _mapper = mapper;
            _partService = partService;
            _requestContextAccessor = requestContextAccessor;
            _userConfigurationService = userConfigurationService;
        }

        /// <summary>
        /// Endpoint that KiCad will initially connect to, to get the endpoint paths and verify the server is accessible
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation($"[KiCadApi] Hello requested from IP '{_requestContextAccessor.GetIpAddress()}'.");
            var orgConfig = _userConfigurationService.GetCachedOrganizationConfiguration();
            if (!orgConfig.KiCad.Enabled) return BadRequest("KiCad HTTP Library not enabled in Settings.");

            return Ok(new
            {
                categories = "", // use default: /kicad-api/v1/categories.json
                parts = "" // use default: /kicad-api/v1/parts/category/{category}.json
            });
        }

        /// <summary>
        /// Get a list of parts for a specified category by its numeric category id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("parts/category/{categoryId}.json")]
        public async Task<IActionResult> GetPartsByCategoryAsync(string categoryId)
        {
            _logger.LogInformation($"[KiCadApi] GET /parts/category/{categoryId}.json from IP '{_requestContextAccessor.GetIpAddress()}'.");
            var orgConfig = _userConfigurationService.GetCachedOrganizationConfiguration();
            if (!orgConfig.KiCad.Enabled) return BadRequest("KiCad HTTP Library not enabled in Settings.");

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
        /// Get part details for a specified part by its numeric part id
        /// </summary>
        /// <param name="partId"></param>
        /// <returns></returns>
        /// <remarks>Called when symbol is clicked on in the Symbol Manager. It's cached between loads.</remarks>
        [HttpGet("parts/{partId}.json")]
        public async Task<IActionResult> GetDetailedPartsByCategoryAsync(string partId)
        {
            _logger.LogInformation($"[KiCadApi] GET /parts/{partId}.json from IP '{_requestContextAccessor.GetIpAddress()}'.");
            var orgConfig = _userConfigurationService.GetCachedOrganizationConfiguration();
            if (!orgConfig.KiCad.Enabled) return BadRequest("KiCad HTTP Library not enabled in Settings.");

            var partIdInt = 0;
            if (!int.TryParse(partId, out partIdInt))
                return NotFound("Invalid part id");
            var part = await _partService.GetPartAsync(new GetPartRequest { PartId = partIdInt });
            if (part == null)
            {
                _logger.LogWarning($"[KiCadApi] GET /parts/{partId}.json part not found from IP '{_requestContextAccessor.GetIpAddress()}'.");
                return NotFound();
            }

            var partType = await _partService.GetPartTypeAsync(part.PartTypeId);
            if (partType == null)
            {
                _logger.LogWarning($"[KiCadApi] GET /parts/{partId}.json part type not found from IP '{_requestContextAccessor.GetIpAddress()}'.");
            }

            var partDetail = _mapper.Map<Part, KiCadPartDetail>(part);

            var referenceDesignator = partType?.ReferenceDesignator;
            if (string.IsNullOrEmpty(referenceDesignator) && !string.IsNullOrEmpty(part.SymbolName))
                referenceDesignator = part.SymbolName[0].ToString();
            if (string.IsNullOrEmpty(referenceDesignator))
                referenceDesignator = "U"; // default if not specified

            // I'm guessing here they expect the category/part type for this value. It doesn't seem to do anything.
            if (string.IsNullOrEmpty(partDetail.SymbolIdStr))
                partDetail.SymbolIdStr = partType?.SymbolId ?? partType?.Name ?? "Unknown";

            foreach (var field in orgConfig.KiCad.ExportFields)
            {
                if (field.Enabled)
                {
                    try
                    {
                        switch (field.Field.ToLower())
                        {
                            case "referencedesignator":
                                CreateField(field.KiCadFieldName, referenceDesignator); // "U"
                                break;
                            case "partnumber":
                                CreateField(field.KiCadFieldName, part.PartNumber, true); // force it to be displayed
                                break;
                            case "parttype":
                                CreateField(field.KiCadFieldName, partType?.Name ?? "Unknown");
                                break;
                            case "value":
                                CreateField(field.KiCadFieldName, part.Value, true); // "1k", force it to be displayed
                                break;
                            case "keywords":
                                if (part.Keywords != null && part.Keywords.Any())
                                {
                                    var keywords = string.Join(" ", part.Keywords);
                                    CreateField(field.KiCadFieldName, keywords);
                                }
                                break;
                            case "quantity":
                                CreateField(field.KiCadFieldName, part.Quantity.ToString());
                                break;
                            default:
                                if (part.ContainsProperty(field.Field))
                                    CreateField(field.KiCadFieldName, part.GetPropertyValue(field.Field));
                                else
                                    _logger.LogWarning($"Unable to map inventory field `{field.Field}` as it does not exist!");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error trying to map inventory field '{field.Field}' to KiCad field '{field.KiCadFieldName}' but encountered an exception! {ex.GetBaseException().Message}");
                    }

                }
            }

            void CreateField(string fieldName, object? fieldValue, bool visible = false)
            {
                if (string.IsNullOrEmpty(fieldName)) return;
                if (string.IsNullOrEmpty(fieldValue?.ToString())) return;
                if (!string.IsNullOrEmpty(fieldValue.ToString()))
                    partDetail.Fields.Add(fieldName, new KiCadValueVisibleItem(fieldValue.ToString(), visible));
            }

            _logger.LogInformation($"[KiCadApi] GET /parts/{partId}.json OK from IP '{_requestContextAccessor.GetIpAddress()}'.");
            return Ok(partDetail);
        }

        /// <summary>
        /// Get a list of categories
        /// </summary>
        /// <returns></returns>
        /// <remarks>Called only on KiCad startup</remarks>
        [HttpGet("categories.json")]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            _logger.LogInformation($"[KiCadApi] GET /categories.json from IP '{_requestContextAccessor.GetIpAddress()}'.");
            var orgConfig = _userConfigurationService.GetCachedOrganizationConfiguration();
            if (!orgConfig.KiCad.Enabled) return BadRequest("KiCad HTTP Library not enabled in Settings.");


            var partTypes = await _partService.GetPartTypesAsync(true);
            if (partTypes == null) return NotFound();

            var partResponse = _mapper.Map<ICollection<PartType>, ICollection<KiCadCategory>>(partTypes);
            _logger.LogInformation($"[KiCadApi] GET /categories.json returned {partResponse.Count} categories from IP '{_requestContextAccessor.GetIpAddress()}'.");
            return Ok(partResponse);
        }
    }
}
