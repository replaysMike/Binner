using Binner.Common.Integrations;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations;
using Binner.Model.Integrations.Element14;
using Binner.Services.Integrations;
using Binner.Services.Integrations.ResponseProcessors;
using Microsoft.Extensions.Logging;

namespace Binner.Services.Integrations.ResponseProcessors
{
    public class Element14PartInfoResponseProcessor : IResponseProcessor
    {
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogger _logger;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly int _resultsRank;

        public Element14PartInfoResponseProcessor(ILogger logger, WebHostServiceConfiguration configuration, int resultsRank)
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

        private async Task<Element14SearchResult?> FetchPartsAsync(IIntegrationApi api, ProcessingContext context)
        {
            IApiResponse? apiResponse = null;
            try
            {
                apiResponse = await api.SearchAsync(context.PartNumber, context.PartType, context.MountingType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(Element14Api)}]: {ex.GetBaseException().Message}");
                apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(Element14Api));
            }
            context.ApiResponses.Add(nameof(Element14Api), new Model.Integrations.ApiResponseState(false, apiResponse));

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

            var Element14Response = (Element14SearchResult?)apiResponse.Response;
            context.ApiResponses[nameof(Element14Api)].IsSuccess = Element14Response?.KeywordSearchReturn?.Products?.Any() == true;
            return Element14Response;
        }

        private Task ToCommonPartAsync(IIntegrationApi api, Element14SearchResult response, ProcessingContext context)
        {
            // check if we have received any parts
            var products = response.KeywordSearchReturn?.Products;
            if (products == null || products.Count == 0)
                return Task.CompletedTask;

            var imagesAdded = 0;
            foreach (var part in products)
            {
                var icMounting = part?.attributes?
                    .FirstOrDefault(a => a.attributeLabel?.Equals("IC Mounting", StringComparison.InvariantCultureIgnoreCase) == true)
                    ?.attributeValue;

                var mountingTypeId = MountingType.None;

                // Check if we can get the mounting type from the response
                if (!string.IsNullOrEmpty(icMounting))
                {
                    if (icMounting.Equals("Surface Mount", StringComparison.InvariantCultureIgnoreCase))
                    {
                        mountingTypeId = MountingType.SurfaceMount;
                    }
                    else if (icMounting.Equals("Through Hole", StringComparison.InvariantCultureIgnoreCase)) 
                    {
                        mountingTypeId = MountingType.ThroughHole;
                    }
                }

                var partSeries = string.Empty;
                var series = part?.attributes?
                    .FirstOrDefault(a => a.attributeLabel != null &&
                        a.attributeLabel.IndexOf("series", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    ?.attributeValue;

                if (!string.IsNullOrEmpty(series))
                {
                    partSeries = series;
                }

                var basePart = string.Empty;
                if (!string.IsNullOrEmpty(part?.translatedManufacturerPartNumber) &&
                    part.translatedManufacturerPartNumber.Contains(context.PartNumber, ComparisonType))
                    basePart = context.PartNumber;
                if (string.IsNullOrEmpty(basePart))
                    basePart = context.PartNumber;

                // Minimum order quantity & factory stock
                var minimumOrderQuantity = part?.translatedMinimumOrderQuality ?? 0;
                var factoryStockAvailable = part?.stock?.level ?? 0;

                // Images
                var imageUrl = part?.image?.mainImageURL ?? string.Empty;
                if (!string.IsNullOrEmpty(imageUrl)
                    && !context.Results.ProductImages.Any(x => x.Value?.Equals(imageUrl, ComparisonType) == true)
                    && imagesAdded < ProcessingContext.MaxImagesPerSupplier
                    && context.Results.ProductImages.Count < ProcessingContext.MaxImagesTotal)
                {
                    if (!string.IsNullOrEmpty(part?.translatedManufacturerPartNumber))
                    {
                        context.Results.ProductImages.Add(new NameValuePair<string>(
                            part.translatedManufacturerPartNumber,
                            imageUrl));
                        imagesAdded++;
                    }
                }

                var datasheetUrls = new List<string>();
                if (part?.datasheets != null)
                {
                    foreach (var ds in part.datasheets)
                    {
                        var datasheetUrl = ds?.Url ?? string.Empty;
                        if (!string.IsNullOrEmpty(datasheetUrl)
                            && !context.Results.Datasheets.Any(x => x.Value?.DatasheetUrl.Equals(datasheetUrl, ComparisonType) == true))
                        {
                            var datasheetSource = new DatasheetSource(
                                $"https://{_configuration.ResourceSource}/{ProcessingContext.MissingDatasheetCoverName}",
                                datasheetUrl,
                                part?.translatedManufacturerPartNumber ?? basePart,
                                ds?.Description ?? string.Empty,
                                part?.brandName ?? string.Empty);

                            context.Results.Datasheets.Add(new NameValuePair<DatasheetSource>(
                                part?.translatedManufacturerPartNumber ?? context.PartNumber,
                                datasheetSource));

                            datasheetUrls.Add(datasheetUrl);
                        }
                    }
                }

                var icCasePackage = part?.attributes?
                    .FirstOrDefault(a => a.attributeLabel?.Equals("IC Case / Package", StringComparison.InvariantCultureIgnoreCase) == true)
                    ?.attributeValue;

                var packageType = string.Empty;

                if (!string.IsNullOrEmpty(icCasePackage))
                {
                    packageType = icCasePackage;
                }

                context.Results.Parts.Add(new CommonPart
                {
                    Rank = _resultsRank,
                    SwarmPartNumberManufacturerId = null,
                    Supplier = api.Name,
                    SupplierPartNumber = part?.sku ?? string.Empty,
                    BasePartNumber = basePart,
                    Manufacturer = part?.brandName ?? string.Empty,
                    ManufacturerPartNumber = part?.translatedManufacturerPartNumber ?? string.Empty,
                    Cost = (double)(part?.prices?.OrderBy(x => x?.from).FirstOrDefault()?.cost ?? 0m),
                    Currency = part?.currency ?? "USD",
                    DatasheetUrls = datasheetUrls,
                    Description = part?.productOverview?.description ?? (part?.displayName ?? string.Empty),
                    ImageUrl = imageUrl,
                    PackageType = packageType,
                    MountingTypeId = (int)mountingTypeId,
                    PartType = part?.nationalClassCode ?? string.Empty,
                    ProductUrl = part?.productURL ?? string.Empty,
                    Status = part?.productStatus ?? string.Empty,
                    QuantityAvailable = part?.stock?.level ?? 0,
                    MinimumOrderQuantity = minimumOrderQuantity,
                    FactoryStockAvailable = factoryStockAvailable,
                    FactoryLeadTime = (part?.stock?.leastLeadTime ?? 0).ToString(),
                    Series = partSeries
                });
            }

            return Task.CompletedTask;
        }
    }
}
