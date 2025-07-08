using Binner.Common.Integrations;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Microsoft.Extensions.Logging;

namespace Binner.Services.Integrations.PartInformation
{
    public class ExternalPartInfoService : PartInfoServiceBase, IExternalPartInfoService
    {
        protected readonly WebHostServiceConfiguration _configuration;
        protected readonly IIntegrationApiFactory _integrationApiFactory;
        protected readonly ILogger<ExternalPartInfoService> _logger;
        protected readonly IExternalBarcodeInfoService _externalBarcodeInfoService;
        protected readonly IUserConfigurationService _userConfigurationService;

        public ExternalPartInfoService(WebHostServiceConfiguration configuration, IStorageProvider storageProvider, IIntegrationApiFactory integrationApiFactory, IRequestContextAccessor requestContextAccessor, ILogger<ExternalPartInfoService> logger, IExternalBarcodeInfoService externalBarcodeInfoService, IUserConfigurationService userConfigurationService)
            : base(storageProvider, requestContextAccessor)
        {
            _configuration = configuration;
            _integrationApiFactory = integrationApiFactory;
            _logger = logger;
            _externalBarcodeInfoService = externalBarcodeInfoService;
            _userConfigurationService = userConfigurationService;
        }

        public virtual async Task<IServiceResult<PartResults?>> GetPartInformationAsync(Part? inventoryPart, string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "")
        {
            var response = new PartResults();
            var user = _requestContext.GetUserContext();

            // if we received a barcode scan, try decoding the information using DigiKey's api (if enabled)
            if (IsBarcodeScan(partNumber))
            {
                partNumber = await DecodeBarcode(partNumber, user);
                if (IsBarcodeScan(partNumber))
                    return ServiceResult<PartResults>.Create($"There are no api's configured to handle decoding of barcode scan. Try configuring {nameof(DigikeyApi)} to enable this feature.", nameof(DigikeyApi));
            }

            if (string.IsNullOrEmpty(partNumber))
            {
                // return empty result, invalid request
                return ServiceResult<PartResults>.Create("No part number requested!", "Multiple");
            }

            // fetch all part types
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());

            // fetch part information from enabled API's
            var partInformationProvider = new PartInformationProvider(_integrationApiFactory, _logger, _configuration, _userConfigurationService);
            PartInformationResults? partInfoResults;
            try
            {
                partInfoResults = await partInformationProvider.FetchPartInformationAsync(partNumber, partType, mountingType, supplierPartNumbers, user?.UserId ?? 0, partTypes, inventoryPart);
                if (partInfoResults.PartResults.Parts.Any())
                    response.Parts.AddRange(partInfoResults.PartResults.Parts);
            }
            catch (ApiErrorException ex)
            {
                // fatal error with executing api request
                return ServiceResult<PartResults>.Create(ex.ApiResponse.Errors, ex.ApiResponse.ApiName);
            }
            catch (ApiRequiresAuthenticationException ex)
            {
                // additional authentication is required from an API (oAuth)
                return ServiceResult<PartResults>.Create(true, ex.ApiResponse.RedirectUrl ?? string.Empty, ex.ApiResponse.Errors, ex.ApiResponse.ApiName);
            }

            // if any enabled API's encountered an error, return the error
            if (!partInfoResults.ApiResponses.Any(x => x.Value.IsSuccess))
            {
                if (partInfoResults.ApiResponses.Any(x => x.Value.Response?.Errors.Any() == true))
                {
                    // there are errors, and no successful responses
                    var errors = partInfoResults.ApiResponses
                        .Where(x => x.Value.Response != null && x.Value.Response.Errors.Any())
                        .SelectMany(x => x.Value.Response!.Errors.Select(errorMessage => $"[{x.Value.Response.ApiName}] {errorMessage}")).ToList();
                    var apiNames = partInfoResults.ApiResponses.Where(x => x.Value.Response?.Errors.Any() == true).GroupBy(x => x.Key);
                    var apiName = "Multiple";
                    if (apiNames.Count() == 1) apiName = apiNames.First().Key;
                    return ServiceResult<PartResults>.Create(errors, apiName);
                }
            }

            // If we have the part in inventory, insert any datasheets from the inventory part
            if (inventoryPart != null && !string.IsNullOrEmpty(inventoryPart.DatasheetUrl))
                response.Datasheets.Add(new NameValuePair<DatasheetSource>(inventoryPart.ManufacturerPartNumber ?? string.Empty, new DatasheetSource($"https://{_configuration.ResourceSource}/{MissingDatasheetCoverName}", inventoryPart.DatasheetUrl, inventoryPart.ManufacturerPartNumber ?? string.Empty, inventoryPart.Description ?? string.Empty, inventoryPart.Manufacturer ?? string.Empty)));

            // Apply ranking to order responses by API.
            // Rank order is specified in the PartInformationProvider
            response.Parts = response.Parts
                // return unique results
                .DistinctBy(x => new { Supplier = x.Supplier ?? string.Empty, SupplierPartNumber = x.SupplierPartNumber ?? string.Empty })
                // order by source
                .OrderBy(x => x.Rank)
                .ThenByDescending(x => x.QuantityAvailable)
                .ThenBy(x => x.BasePartNumber)
                .ThenBy(x => x.Status)
                .ToList();

            // map PartIds for local inventory
            response.Parts = await MapCommonPartIdsAsync(response.Parts);

            // Combine all the product images and datasheets into the root response and remove duplicates
            response.ProductImages = partInfoResults.PartResults.ProductImages.DistinctBy(x => x.Value).ToList();
            response.Datasheets = partInfoResults.PartResults.Datasheets.DistinctBy(x => x.Value).ToList();

            // iterate through the responses and inject PartType objects and keywords
            await InjectPartTypesAndKeywordsAsync(response, partTypes);

            var serviceResult = ServiceResult<PartResults>.Create(response);
            if (partInfoResults.ApiResponses.Any(x => x.Value.Response != null && x.Value.Response.Errors.Any()))
                serviceResult.Errors = partInfoResults.ApiResponses
                    .Where(x => x.Value.Response != null && x.Value.Response.Errors.Any())
                    .SelectMany(x => x.Value.Response!.Errors.Select(errorMessage => $"[{x.Value.Response.ApiName}] {errorMessage}"));
            return serviceResult;

            async Task InjectPartTypesAndKeywordsAsync(PartResults response, ICollection<PartType> partTypes)
            {
                foreach (var part in response.Parts)
                {
                    var partType = await DeterminePartTypeAsync(part);
                    part.PartType = partType?.Name ?? string.Empty;
                    part.PartTypeId = partType?.PartTypeId ?? 0;
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }
            }

            async Task<string> DecodeBarcode(string partNumber, IUserContext? user)
            {
                if (user == null) throw new UserContextUnauthorizedException();
                var integrationConfiguration = _userConfigurationService.GetCachedOrganizationIntegrationConfiguration(user.OrganizationId);
                var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user.UserId, integrationConfiguration);
                if (digikeyApi.Configuration.IsConfigured)
                {
                    // 2d barcode scan requires decode first to get the partNumber being searched
                    var barcodeInfo = await _externalBarcodeInfoService.GetBarcodeInfoAsync(partNumber, ScannedLabelType.Product);
                    if (barcodeInfo.Response?.Parts.Any() == true)
                    {
                        var firstPartMatch = barcodeInfo.Response.Parts.First();
                        if (!string.IsNullOrEmpty(firstPartMatch.ManufacturerPartNumber))
                            partNumber = firstPartMatch.ManufacturerPartNumber;
                        else if (!string.IsNullOrEmpty(firstPartMatch.BasePartNumber))
                            partNumber = firstPartMatch.BasePartNumber;
                    }
                }

                return partNumber;
            }
        }

        private bool IsBarcodeScan(string partNumber) => !string.IsNullOrEmpty(partNumber) && partNumber.StartsWith("[)>");
    }
}
