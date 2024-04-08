using Binner.Common.Integrations.Models;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations.Arrow;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Integrations.ResponseProcessors
{
    public class ArrowPartInfoResponseProcessor : IResponseProcessor
    {
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogger _logger;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly int _resultsRank;

        public ArrowPartInfoResponseProcessor(ILogger logger, WebHostServiceConfiguration configuration, int resultsRank)
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

        private async Task<ArrowResponse?> FetchPartsAsync(IIntegrationApi api, ProcessingContext context)
        {
            IApiResponse? apiResponse = null;
            try
            {
                apiResponse = await api.SearchAsync(context.PartNumber, context.PartType, context.MountingType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(ArrowApi)}]: {ex.GetBaseException().Message}");
                apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(ArrowApi));
            }
            context.ApiResponses.Add(nameof(ArrowApi), new Model.Integrations.ApiResponseState(false, apiResponse));
            if (apiResponse.Warnings?.Any() == true)
            {
                _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
            }
            if (apiResponse.Errors?.Any() == true)
            {
                _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
                //return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
            }

            var arrowResponse = (ArrowResponse?)apiResponse.Response;
            context.ApiResponses[nameof(ArrowApi)].IsSuccess = arrowResponse?.ItemServiceResult?.Data?.Any() == true;
            return arrowResponse;
        }

        private Task ToCommonPartAsync(IIntegrationApi api, ArrowResponse response, ProcessingContext context)
        {
            if (response.ItemServiceResult == null || response.ItemServiceResult.Data == null || !response.ItemServiceResult.Data.Any()) return Task.CompletedTask;

            var productImageUrls = new List<NameValuePair<string>>();
            if (response.ItemServiceResult.Data.Any())
            {
                var data = response.ItemServiceResult.Data.FirstOrDefault();
                if (data == null || data.PartList == null || !data.PartList.Any())
                    return Task.CompletedTask;
                foreach (var part in data.PartList)
                {
                    var additionalPartNumbers = new List<string>();
                    var basePart = context.PartNumber;
                    var itemId = part.ItemId;
                    var productStatus = part.Status;
                    var manufacturerPartNumber = part.PartNum ?? context.PartNumber;
                    var mountingTypeId = MountingType.None;
                    /*Enum.TryParse<MountingType>(part.
                        .Where(x => x.Parameter.Equals("Mounting Type", ComparisonType))
                        .Select(x => x.Value?.Replace(" ", ""))
                        .FirstOrDefault(), out mountingTypeId);*/
                    var packageType = part.PackageType;
                    var productUrl = part.Resources.Where(x => x.Type == "cloud_part_detail").Select(x => x.Uri).FirstOrDefault();

                    var images = part.Resources.Where(x => /*x.Type == "image_small" ||*/ x.Type == "image_large");
                    foreach (var image in images)
                    {
                        if (productImageUrls.Count < ProcessingContext.MaxImagesPerSupplier && context.Results.ProductImages.Count < ProcessingContext.MaxImagesTotal && !string.IsNullOrEmpty(image.Uri))
                        {
                            productImageUrls.Add(new NameValuePair<string>(manufacturerPartNumber, image.Uri));
                            context.Results.ProductImages.Add(new NameValuePair<string>(manufacturerPartNumber, image.Uri));
                        }
                    }

                    if (productImageUrls.Count == 0)
                    {
                        images = part.Resources.Where(x => x.Type == "image_small");
                        foreach (var image in images)
                        {
                            if (productImageUrls.Count < ProcessingContext.MaxImagesPerSupplier && context.Results.ProductImages.Count < ProcessingContext.MaxImagesTotal && !string.IsNullOrEmpty(image.Uri))
                            {
                                productImageUrls.Add(new NameValuePair<string>(manufacturerPartNumber, image.Uri));
                                context.Results.ProductImages.Add(new NameValuePair<string>(manufacturerPartNumber, image.Uri));
                            }
                        }
                    }

                    var arrowDatasheets = part.Resources
                        .Where(x => x.Type == "datasheet" && !string.IsNullOrEmpty(x.Uri))
                        .Select(x => x.Uri ?? string.Empty)
                        .ToList();
                    foreach (var datasheetUri in arrowDatasheets)
                    {
                        var datasheetSource = new DatasheetSource($"https://{_configuration.ResourceSource}/{ProcessingContext.MissingDatasheetCoverName}", datasheetUri,
                            manufacturerPartNumber, "", part.Manufacturer?.MfrName ?? string.Empty);
                        context.Results.Datasheets.Add(new NameValuePair<DatasheetSource>(manufacturerPartNumber, datasheetSource));
                    }

                    var source = part.InvOrg?.WebSites
                        .Where(x => x.Code == "arrow.com")
                        .SelectMany(x => x.Sources)
                        .FirstOrDefault(x => x.SourceCd == "ACNA");

                    var partCost = 0d;
                    var minimumOrderQuantity = 1;
                    var quantityAvailable = 0;
                    var currency = source?.Currency ?? _configuration.Locale.Currency.ToString().ToUpper();
                    if (source != null)
                    {
                        var sourcePart = source.SourceParts
                            .OrderBy(x => x.MinimumOrderQuantity)
                            .FirstOrDefault();
                        if (sourcePart != null)
                        {
                            minimumOrderQuantity = sourcePart.MinimumOrderQuantity;
                            var price = sourcePart.Prices.ResaleList
                                .OrderBy(x => x.MinQty)
                                .FirstOrDefault();
                            partCost = price?.Price ?? 0;

                            quantityAvailable = sourcePart.Availability
                                .OrderByDescending(x => x.FohQty)
                                .Select(x => x.FohQty)
                                .FirstOrDefault();
                        }
                    }

                    context.Results.Parts.Add(new CommonPart
                    {
                        Rank = _resultsRank,
                        SwarmPartNumberManufacturerId = null,
                        Supplier = api.Name,
                        SupplierPartNumber = part.PartNum, // todo:
                        BasePartNumber = basePart,
                        AdditionalPartNumbers = additionalPartNumbers,
                        Manufacturer = part.Manufacturer?.MfrName,
                        ManufacturerPartNumber = manufacturerPartNumber,
                        Cost = partCost,
                        Currency = currency,
                        DatasheetUrls = arrowDatasheets,
                        Description = part.Desc,
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
