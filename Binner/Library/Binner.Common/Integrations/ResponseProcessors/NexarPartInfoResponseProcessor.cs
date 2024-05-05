using Binner.Common.Integrations.Models;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations.Nexar;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Integrations.ResponseProcessors
{
    public class NexarPartInfoResponseProcessor : IResponseProcessor
    {
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogger _logger;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly int _resultsRank;

        public NexarPartInfoResponseProcessor(ILogger logger, WebHostServiceConfiguration configuration, int resultsRank)
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

        private async Task<NexarPartResults?> FetchPartsAsync(IIntegrationApi api, ProcessingContext context)
        {
            IApiResponse? apiResponse = null;
            try
            {
                apiResponse = await api.SearchAsync(context.PartNumber, context.PartType, context.MountingType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(NexarApi)}]: {ex.GetBaseException().Message}");
                apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(NexarApi));
            }
            context.ApiResponses.Add(nameof(NexarApi), new Model.Integrations.ApiResponseState(false, apiResponse));

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
                //return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
            }
            var nexarResponse = (NexarPartResults?)apiResponse.Response;
            context.ApiResponses[nameof(NexarApi)].IsSuccess = nexarResponse?.Parts?.Any() == true;
            return nexarResponse;
        }

        private Task ToCommonPartAsync(IIntegrationApi api, NexarPartResults response, ProcessingContext context)
        {
            if (response.Parts == null || !response.Parts.Any()) return Task.CompletedTask;

            var productImageUrls = new List<NameValuePair<string>>();
            if (response.Parts.Any())
            {
                foreach (var part in response.Parts)
                {
                    var additionalPartNumbers = new List<string>();
                    var basePart = context.PartNumber;
                    var productStatus = ""; // todo:
                    var manufacturerPartNumber = part.ManufacturerPartNumber ?? context.PartNumber;
                    var mountingTypeId = MountingType.None;
                    /*Enum.TryParse<MountingType>(part.
                        .Where(x => x.Parameter.Equals("Mounting Type", ComparisonType))
                        .Select(x => x.Value?.Replace(" ", ""))
                        .FirstOrDefault(), out mountingTypeId);*/
                    var packageType = part.Specs.Where(x => x.Attribute?.ShortName == "case_package")
                        .Select(x => x.Value).FirstOrDefault();
                    var productUrl = "";

                    if (!string.IsNullOrEmpty(part.BestImageUrl))
                        context.Results.ProductImages.Add(new NameValuePair<string>(manufacturerPartNumber, part.BestImageUrl));
                    foreach (var image in part.Images)
                    {
                        if (productImageUrls.Count < ProcessingContext.MaxImagesPerSupplier && context.Results.ProductImages.Count < ProcessingContext.MaxImagesTotal && !string.IsNullOrEmpty(image.Url))
                        {
                            productImageUrls.Add(new NameValuePair<string>(manufacturerPartNumber, image.Url));
                            context.Results.ProductImages.Add(new NameValuePair<string>(manufacturerPartNumber, image.Url));
                        }
                    }

                    if (part.BestDatasheet != null && !string.IsNullOrEmpty(part.BestDatasheet.Url))
                        context.Results.Datasheets.Add(new NameValuePair<DatasheetSource>(manufacturerPartNumber, new DatasheetSource($"https://{_configuration.ResourceSource}/{ProcessingContext.MissingDatasheetCoverName}", part.BestDatasheet.Url, manufacturerPartNumber, "", part.Manufacturer?.Name ?? string.Empty)));
                    var nexarDatasheets = part.Documents
                        .Where(x => x.Name == "Datasheet" && !string.IsNullOrEmpty(x.Url))
                        .ToList();
                    foreach (var datasheetUri in nexarDatasheets)
                    {
                        if (!string.IsNullOrEmpty(datasheetUri.Url))
                        {
                            var datasheetSource = new DatasheetSource(Guid.Empty,
                                0,
                                datasheetUri.PageCount,
                                $"https://{_configuration.ResourceSource}/{ProcessingContext.MissingDatasheetCoverName}",
                                datasheetUri.Url,
                                manufacturerPartNumber,
                                "",
                                part.Manufacturer?.Name,
                                datasheetUri.SourceUrl,
                                null);
                            context.Results.Datasheets.Add(new NameValuePair<DatasheetSource>(manufacturerPartNumber, datasheetSource));
                        }
                    }

                    var partCost = part.MedianPrice1000?.Price ?? 0d;
                    var minimumOrderQuantity = 1;
                    var quantityAvailable = part.TotalAvail;
                    var currency = part.MedianPrice1000?.Currency ?? _configuration.Locale.Currency.ToString().ToUpper();

                    if (part.Sellers.Any())
                    {
                        // add each seller from Octopart
                        foreach (var seller in part.Sellers)
                        {
                            var offer = seller.Offers.OrderBy(x => x.Moq).FirstOrDefault();
                            context.Results.Parts.Add(new CommonPart
                            {
                                Rank = _resultsRank,
                                SwarmPartNumberManufacturerId = null,
                                Supplier = seller.Company?.Name ?? string.Empty,
                                /*SupplierPartNumber = part.PartNum, // todo:*/
                                BasePartNumber = basePart,
                                AdditionalPartNumbers = additionalPartNumbers,
                                Manufacturer = part.ManufacturerName,
                                ManufacturerPartNumber = manufacturerPartNumber,
                                Cost = partCost,
                                Currency = currency,
                                DatasheetUrls = nexarDatasheets.Select(x => x.Url ?? string.Empty).ToList(),
                                Description = part.ShortDescription,
                                ImageUrl = productImageUrls.Select(x => x.Value).FirstOrDefault(),
                                PackageType = packageType,
                                MountingTypeId = (int)mountingTypeId,
                                PartType = "",
                                ProductUrl = offer?.Url,
                                Status = productStatus,
                                QuantityAvailable = offer?.InventoryLevel ?? 0,
                                MinimumOrderQuantity = offer?.Moq ?? 0,
                                //FactoryStockAvailable = factoryStockAvailable,
                                //FactoryLeadTime = part.LeadTime
                            });
                        }
                    }
                    else
                    {
                        // there are no suppliers listed. Show it as an Octopart part
                        context.Results.Parts.Add(new CommonPart
                        {
                            Rank = _resultsRank,
                            SwarmPartNumberManufacturerId = null,
                            Supplier = api.Name,
                            /*SupplierPartNumber = part.PartNum, // todo:*/
                            BasePartNumber = basePart,
                            AdditionalPartNumbers = additionalPartNumbers,
                            Manufacturer = part.ManufacturerName,
                            ManufacturerPartNumber = manufacturerPartNumber,
                            Cost = partCost,
                            Currency = currency,
                            DatasheetUrls = nexarDatasheets.Select(x => x.Url ?? string.Empty).ToList(),
                            Description = part.ShortDescription,
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
            }


            return Task.CompletedTask;
        }
    }
}
