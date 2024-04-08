using Binner.Common.Integrations.Models;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations.Tme;
using Microsoft.Extensions.Logging;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.SS.Formula.UDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypeSupport.Extensions;

namespace Binner.Common.Integrations.ResponseProcessors
{
    public class TmePartInfoResponseProcessor : IResponseProcessor
    {
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogger _logger;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly int _resultsRank;

        public TmePartInfoResponseProcessor(ILogger logger, WebHostServiceConfiguration configuration, int resultsRank)
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

        private async Task<TmeResponse<ProductSearchResponse>?> FetchPartsAsync(IIntegrationApi api, ProcessingContext context)
        {
            IApiResponse? apiResponse = null;
            try
            {
                apiResponse = await api.SearchAsync(context.PartNumber, context.PartType, context.MountingType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(TmeApi)}]: {ex.GetBaseException().Message}");
                apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(TmeApi));
            }
            context.ApiResponses.Add(nameof(TmeApi), new Model.Integrations.ApiResponseState(false, apiResponse));
            if (apiResponse.Warnings?.Any() == true)
            {
                _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
            }
            if (apiResponse.Errors?.Any() == true)
            {
                _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
            }

            var tmeResponse = (TmeResponse<ProductSearchResponse>?)apiResponse.Response;

            if (tmeResponse?.Data?.ProductList.Any() == true)
            {
                context.ApiResponses[nameof(TmeApi)].IsSuccess = true;

                // for subsequent api calls required, get up to 50 Symbols (supplier part numbers) and call other apis for more info
                var partNumbers = tmeResponse.Data.ProductList
                    .Select(x => x.Symbol ?? string.Empty)
                    .Distinct()
                    .Take(50) // max 50 symbols can be fetched for TME api
                    .ToList();

                // first api call doesn't contain pricing information
                // fetch pricing information from api
                var pricesResponse = await ((TmeApi)api).GetProductPricesAsync(partNumbers);
                if (pricesResponse != null && pricesResponse.Response != null)
                {
                    var response = ((TmeResponse<PriceListResponse>)pricesResponse.Response)?.Data;
                    if (response != null)
                    {
                        var productPriceList = response.ProductList;
                        var currency = response.Currency ?? "USD";
                        if (productPriceList != null)
                        {
                            foreach(var priceList in productPriceList)
                            {
                                var target = tmeResponse.Data.ProductList
                                    .Where(x => x.Symbol == priceList.Symbol)
                                    .FirstOrDefault();
                                if (target != null)
                                {
                                    // set pricing information
                                    target.QuantityAvailable = priceList.Amount;
                                    target.Currency = currency;
                                    target.Cost = priceList.PriceList.OrderBy(x => x.Amount).Select(x => x.PriceValue).FirstOrDefault();
                                }
                            }
                        }
                    }
                }

                // first api call doesn't contain datasheets or photos
                // fetch datasheets/images/files information from api
                var filesResponse = await ((TmeApi)api).GetProductFilesAsync(partNumbers);
                if (filesResponse != null && filesResponse.Response != null)
                {
                    var productList = ((TmeResponse<ProductFilesResponse>)filesResponse.Response)?.Data?.ProductList;
                    if (productList != null)
                    {
                        foreach (var documentList in productList)
                        {
                            var target = tmeResponse.Data.ProductList
                                .Where(x => x.Symbol == documentList.Symbol)
                                .FirstOrDefault();
                            if (target != null)
                            {
                                var datasheets = documentList.Files.DocumentList
                                    .Where(x => x.DocumentType == DocumentTypes.DTE)
                                    .ToList();
                                var videos = documentList.Files.DocumentList
                                    .Where(x => x.DocumentType == DocumentTypes.YTB)
                                    .ToList();
                                var photos = documentList.Files.PhotoList.Select(x => new TmePhotoFile(x, TmePhotoResolution.Default)).ToList();
                                photos.AddRange(documentList.Files.ThumbnailList.Select(x => new TmePhotoFile(x, TmePhotoResolution.Thumbnail)));
                                photos.AddRange(documentList.Files.HighResolutionPhotoList.Select(x => new TmePhotoFile(x, TmePhotoResolution.High)));

                                target.Datasheets = datasheets;
                                target.Videos = videos;
                                target.Photos = photos;
                            }
                        }
                    }
                }
            }

            return tmeResponse;
        }

        private Task ToCommonPartAsync(IIntegrationApi api, TmeResponse<ProductSearchResponse> response, ProcessingContext context)
        {
            if (response.Data == null || !response.Data.ProductList.Any()) return Task.CompletedTask;

            var productImageUrls = new List<NameValuePair<string>>();
            var datasheetUrls = new List<string>();
            if (response.Data.ProductList.Any())
            {
                var data = response.Data.ProductList;
                if (data == null)
                    return Task.CompletedTask;
                foreach (var part in data)
                {
                    var additionalPartNumbers = new List<string>();
                    var basePart = part.Symbol ?? context.PartNumber;
                    var productStatus = string.Join(",", part.ProductStatusList);
                    var manufacturerPartNumber = part.OriginalSymbol ?? part.Symbol ?? context.PartNumber;
                    var mountingTypeId = MountingType.None;
                    /*Enum.TryParse<MountingType>(part.
                        .Where(x => x.Parameter.Equals("Mounting Type", ComparisonType))
                        .Select(x => x.Value?.Replace(" ", ""))
                        .FirstOrDefault(), out mountingTypeId);*/
                    var packageType = string.Empty;
                    var productUrl = "https:" + part.ProductInformationPage;

                    if (!string.IsNullOrEmpty(part.Photo))
                    {
                        var imageUrl = new NameValuePair<string>(manufacturerPartNumber, "https:" + part.Photo);
                        if (!productImageUrls.Contains(imageUrl))
                            productImageUrls.Add(imageUrl);
                        if (!context.Results.ProductImages.Contains(imageUrl))
                            context.Results.ProductImages.Add(imageUrl);
                    }

                    if (!string.IsNullOrEmpty(part.Thumbnail))
                    {
                        var imageUrl = new NameValuePair<string>(manufacturerPartNumber, "https:" + part.Thumbnail);
                        if (!productImageUrls.Contains(imageUrl))
                            productImageUrls.Add(imageUrl);
                        if (!context.Results.ProductImages.Contains(imageUrl))
                            context.Results.ProductImages.Add(imageUrl);
                    }

                    foreach(var imageUri in part.Photos)
                    {
                        var imageUrl = new NameValuePair<string>(manufacturerPartNumber, "https:" + imageUri.Photo);
                        if (!productImageUrls.Contains(imageUrl))
                            productImageUrls.Add(imageUrl);
                        if (!context.Results.ProductImages.Contains(imageUrl))
                            context.Results.ProductImages.Add(imageUrl);
                    }

                    var tmeDatasheets = part.Datasheets
                        .Where(x => !string.IsNullOrEmpty(x.DocumentUrl))
                        .Select(x => x.DocumentUrl ?? string.Empty)
                        .ToList();
                    foreach (var datasheetUri in tmeDatasheets)
                    {
                        var uri = "https:" + datasheetUri;
                        var datasheetSource = new DatasheetSource($"https://{_configuration.ResourceSource}/{ProcessingContext.MissingDatasheetCoverName}", uri,
                            manufacturerPartNumber, "", part.Producer ?? string.Empty);
                        if (!datasheetUrls.Contains(datasheetSource.DatasheetUrl))
                            datasheetUrls.Add(datasheetSource.DatasheetUrl);

                        var datasheetUrl = new NameValuePair<DatasheetSource>(manufacturerPartNumber, datasheetSource);
                        if (!context.Results.Datasheets.Contains(datasheetUrl))
                            context.Results.Datasheets.Add(datasheetUrl);
                    }

                    var partCost = part.Cost;
                    var minimumOrderQuantity = part.MinAmount;
                    var quantityAvailable = part.QuantityAvailable;
                    var currency = string.IsNullOrEmpty(part.Currency) ? _configuration.Locale.Currency.ToString().ToUpper() : part.Currency;

                    context.Results.Parts.Add(new CommonPart
                    {
                        Rank = _resultsRank,
                        SwarmPartNumberManufacturerId = null,
                        Supplier = api.Name,
                        SupplierPartNumber = part.Symbol,
                        BasePartNumber = basePart,
                        AdditionalPartNumbers = additionalPartNumbers,
                        Manufacturer = part.Producer,
                        ManufacturerPartNumber = manufacturerPartNumber,
                        Cost = partCost,
                        Currency = currency,
                        DatasheetUrls = datasheetUrls.ToList(),
                        Description = part.Description,
                        ImageUrl = productImageUrls.Select(x => x.Value).FirstOrDefault(),
                        PackageType = packageType,
                        MountingTypeId = (int)mountingTypeId,
                        PartType = "",
                        ProductUrl = productUrl,
                        Status = productStatus,
                        QuantityAvailable = quantityAvailable,
                        MinimumOrderQuantity = minimumOrderQuantity,
                        //FactoryStockAvailable = factoryStockAvailable,
                        //FactoryLeadTime = part.LeadTime
                    });
                }
            }


            return Task.CompletedTask;
        }
    }
}
