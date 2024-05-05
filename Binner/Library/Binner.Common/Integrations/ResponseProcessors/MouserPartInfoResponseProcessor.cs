using Binner.Common.Integrations.Models;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations.Mouser;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Integrations.ResponseProcessors
{
    public class MouserPartInfoResponseProcessor : IResponseProcessor
    {
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogger _logger;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly int _resultsRank;

        public MouserPartInfoResponseProcessor(ILogger logger, WebHostServiceConfiguration configuration, int resultsRank)
        {
            _logger = logger;
            _configuration = configuration;
            _resultsRank = resultsRank;
        }

        public async Task ExecuteAsync(IIntegrationApi api, ProcessingContext context)
        {
            // fetch part info
            var response = await FetchPartsAsync(api, context);
            // merge response
            if (response != null)
                await ToCommonPartAsync(api, response, context);
        }

        private async Task<SearchResultsResponse?> FetchPartsAsync(IIntegrationApi api, ProcessingContext context)
        {
            IApiResponse? apiResponse = null;
            try
            {
                apiResponse = await api.SearchAsync(context.PartNumber, context.PartType, context.MountingType);
            }
            catch (MouserErrorsException ex)
            {
                _logger.LogError(ex, $"[{nameof(MouserApi)}]: {string.Join(", ", ex.Errors)}");
                apiResponse = new ApiResponse(ex.Errors.Select(x => x.Message ?? string.Empty).ToList(), nameof(MouserApi));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(MouserApi)}]: {ex.GetBaseException().Message}");
                apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(MouserApi));
            }
            context.ApiResponses.Add(nameof(MouserApi), new Model.Integrations.ApiResponseState(false, apiResponse));

            if (apiResponse.RequiresAuthentication)
            {
                throw new ApiRequiresAuthenticationException(apiResponse);
            }

            if (apiResponse.Warnings?.Any() == true)
            {
                _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
            }
            if (apiResponse.Errors?.Any() == true)
            {
                _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
            }

            var mouserResponse = (SearchResultsResponse?)apiResponse.Response;
            context.ApiResponses[nameof(MouserApi)].IsSuccess = mouserResponse?.SearchResults?.Parts?.Any() == true;
            return mouserResponse;
        }

        private Task ToCommonPartAsync(IIntegrationApi api, SearchResultsResponse response, ProcessingContext context)
        {
            if (response.SearchResults == null) return Task.CompletedTask;
            var imagesAdded = 0;
            if (response.SearchResults.Parts != null)
            {
                foreach (var part in response.SearchResults.Parts)
                {
                    var mountingTypeId = MountingType.None;
                    var basePart = string.Empty;
                    if (part.ManufacturerPartNumber?.Contains(context.PartNumber, ComparisonType) == true)
                        basePart = context.PartNumber;
                    if (string.IsNullOrEmpty(basePart))
                        basePart = context.PartNumber;

                    var currency = part.PriceBreaks?.OrderBy(x => x.Quantity).FirstOrDefault()?.Currency;
                    if (string.IsNullOrEmpty(currency))
                        currency = _configuration.Locale.Currency.ToString().ToUpper();
                    int.TryParse(part.Min, out var minimumOrderQuantity);
                    int.TryParse(part.FactoryStock, out var factoryStockAvailable);
                    if (!string.IsNullOrEmpty(part.ImagePath)
                        && !context.Results.ProductImages.Any(x => x.Value?.Equals(part.ImagePath, ComparisonType) == true)
                        && imagesAdded < ProcessingContext.MaxImagesPerSupplier && context.Results.ProductImages.Count < ProcessingContext.MaxImagesTotal)
                    {
                        if (!string.IsNullOrEmpty(part.ManufacturerPartNumber))
                        {
                            context.Results.ProductImages.Add(new NameValuePair<string>(part.ManufacturerPartNumber,
                                part.ImagePath));
                            imagesAdded++;
                        }
                    }

                    // if there is a datasheet that hasn't been added, add it
                    if (!string.IsNullOrEmpty(part.DataSheetUrl)
                        && !context.Results.Datasheets.Any(x => x.Value?.DatasheetUrl.Equals(part.DataSheetUrl, ComparisonType) == true))
                    {
                        var datasheetSource = new DatasheetSource($"https://{_configuration.ResourceSource}/{ProcessingContext.MissingDatasheetCoverName}", part.DataSheetUrl,
                            part.ManufacturerPartNumber ?? basePart, "", part.Manufacturer ?? string.Empty);
                        context.Results.Datasheets.Add(new NameValuePair<DatasheetSource>(part.ManufacturerPartNumber ?? context.PartNumber, datasheetSource));
                    }

                    context.Results.Parts.Add(new CommonPart
                    {
                        Rank = _resultsRank,
                        SwarmPartNumberManufacturerId = null,
                        Supplier = api.Name,
                        SupplierPartNumber = part.MouserPartNumber,
                        BasePartNumber = basePart,
                        Manufacturer = part.Manufacturer,
                        ManufacturerPartNumber = part.ManufacturerPartNumber,
                        Cost = (part.PriceBreaks?.OrderBy(x => x.Quantity).FirstOrDefault()?.Cost) ?? 0d,
                        Currency = currency,
                        DatasheetUrls = new List<string> { part.DataSheetUrl ?? string.Empty },
                        Description = part.Description,
                        ImageUrl = part.ImagePath,
                        PackageType = "",
                        MountingTypeId = (int)mountingTypeId,
                        PartType = "",
                        ProductUrl = part.ProductDetailUrl,
                        Status = part.LifecycleStatus,
                        QuantityAvailable = part.AvailabilityInteger,
                        MinimumOrderQuantity = minimumOrderQuantity,
                        FactoryStockAvailable = factoryStockAvailable,
                        FactoryLeadTime = part.LeadTime
                    });
                }
            }

            return Task.CompletedTask;
        }
    }
}
