using Binner.Common.Integrations.Models;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations.DigiKey;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Binner.Common.Integrations.ResponseProcessors
{
    public class DigiKeyPartInfoResponseProcessor : IResponseProcessor
    {
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogger _logger;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly int _resultsRank;

        public DigiKeyPartInfoResponseProcessor(ILogger logger, WebHostServiceConfiguration configuration, int resultsRank)
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

        private async Task<KeywordSearchResponse?> FetchPartsAsync(IIntegrationApi api, ProcessingContext context)
        {
            IApiResponse? apiResponse = null;
            try
            {
                apiResponse = await api.SearchAsync(context.PartNumber, context.PartType, context.MountingType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(DigikeyApi)}]: {ex.GetBaseException().Message}");
                apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(DigikeyApi));
            }
            context.ApiResponses.Add(nameof(DigikeyApi), new Model.Integrations.ApiResponseState(false, apiResponse));

            if (apiResponse.RequiresAuthentication)
            {
                throw new ApiRequiresAuthenticationException(apiResponse);
            }

            if (apiResponse.Warnings.Any())
            {
                _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
            }
            if (apiResponse.Errors.Any())
            {
                _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
                // return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
            }

            var digikeyResponse = (KeywordSearchResponse?)apiResponse.Response;
            if (digikeyResponse != null)
            {
                // if no part found, and it's numerical, try getting barcode info
                if (digikeyResponse.Products.Any() != true)
                {
                    var isNumber = Regex.IsMatch(context.PartNumber, @"^\d+$");
                    if (isNumber)
                    {
                        var barcode = context.PartNumber;
                        IServiceResult<Product?>? barcodeResult = null;
                        try
                        {
                            barcodeResult = await GetBarcodeInfoProductAsync(api, barcode, ScannedBarcodeType.Product);
                            digikeyResponse = new KeywordSearchResponse();
                            if (barcodeResult?.Response != null)
                                digikeyResponse.Products.Add(barcodeResult.Response);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"[{nameof(DigikeyApi)}]: {ex.GetBaseException().Message}");
                            apiResponse.Errors.Add($"Error fetching barcode info on '{WebUtility.HtmlEncode(barcode)}': {ex.GetBaseException().Message}");
                        }
                    }
                }

                // if no part found, look up part if a supplier part number is provided
                if (digikeyResponse.Products.Any() != true && !string.IsNullOrEmpty(context.SupplierPartNumbers))
                {
                    var supplierPartNumberParts = context.SupplierPartNumbers.Split(',');
                    foreach (var supplierPartNumberPair in supplierPartNumberParts)
                    {
                        var supplierPair = supplierPartNumberPair.Split(':');
                        if (supplierPair.Length < 2) continue;
                        var supplierName = supplierPair[0];
                        if (supplierName.Equals("digikey", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var supplierPartNumber = supplierPair[1];
                            if (!string.IsNullOrEmpty(supplierPartNumber))
                            {
                                // try looking it up via the digikey part number
                                IApiResponse? partResponse = null;
                                try
                                {
                                    partResponse = await api.GetProductDetailsAsync(supplierPartNumber);
                                    if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                                    {
                                        var part = (Product?)partResponse.Response;
                                        if (part != null)
                                            digikeyResponse.Products.Add(part);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"[{nameof(DigikeyApi)}]: {ex.GetBaseException().Message}");
                                    apiResponse.Errors.Add($"Error fetching product details on '{WebUtility.HtmlEncode(supplierPartNumber)}': {ex.GetBaseException().Message}");
                                }
                            }
                        }
                    }
                }

                // if no part found, try treating the part number as a supplier part number
                if (digikeyResponse.Products.Any() != true && !string.IsNullOrEmpty(context.SupplierPartNumbers))
                {
                    var supplierPartNumber = context.PartNumber;
                    // try looking it up via the digikey part number
                    try
                    {
                        var partResponse = await api.GetProductDetailsAsync(supplierPartNumber);
                        if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                        {
                            var part = (Product?)partResponse.Response;
                            if (part != null)
                                digikeyResponse.Products.Add(part);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[{nameof(DigikeyApi)}]: {ex.GetBaseException().Message}");
                        apiResponse.Errors.Add($"Error fetching product details on supplier part number '{WebUtility.HtmlEncode(context.SupplierPartNumbers)}': {ex.GetBaseException().Message}");

                    }
                }

                context.ApiResponses[nameof(DigikeyApi)].IsSuccess = digikeyResponse.Products.Any();
            }

            return digikeyResponse;
        }

        private Task ToCommonPartAsync(IIntegrationApi api, KeywordSearchResponse response, ProcessingContext context)
        {
            if (!response.Products.Any()) return Task.CompletedTask;

            var imagesAdded = 0;
            foreach (var part in response.Products)
            {
                var additionalPartNumbers = new List<string>();
                if (part.Parameters != null)
                {
                    var basePart = part.Parameters
                        .Where(x => x.Parameter.Equals("Base Part Number", ComparisonType))
                        .Select(x => x.Value)
                        .FirstOrDefault();
                    if (!string.IsNullOrEmpty(basePart))
                        additionalPartNumbers.Add(basePart);
                    else
                    {
                        if (part.ManufacturerPartNumber.Contains(context.PartNumber, ComparisonType))
                            basePart = context.PartNumber;
                    }

                    if (string.IsNullOrEmpty(basePart))
                        basePart = part.ManufacturerPartNumber;
                    var mountingTypeId = MountingType.None;
                    var mountingTypeParameter = part.Parameters
                        .Where(x => x.Parameter.Equals("Mounting Type", ComparisonType))
                        .Select(x => x.Value?.Replace(" ", ""))
                        .FirstOrDefault();

                    if (mountingTypeParameter?.Contains(",") == true)
                    {
                        // DigiKey very rarely returns a part as being more than one mounting type. Pick the last one.
                        mountingTypeParameter = mountingTypeParameter.Split(",", StringSplitOptions.RemoveEmptyEntries).Last();
                    }

                    Enum.TryParse<MountingType>(mountingTypeParameter, out mountingTypeId);
                    var currency = response.SearchLocaleUsed.Currency;
                    if (string.IsNullOrEmpty(currency))
                        currency = _configuration.Locale.Currency.ToString().ToUpper();
                    var packageType = part.Parameters
                        ?.Where(x => x.Parameter.Equals("Supplier Device Package", ComparisonType))
                        .Select(x => x.Value)
                        .FirstOrDefault();
                    if (string.IsNullOrEmpty(packageType))
                        packageType = part.Parameters
                            ?.Where(x => x.Parameter.Equals("Package / Case", ComparisonType))
                            .Select(x => x.Value)
                            .FirstOrDefault();
                    if (!string.IsNullOrEmpty(part.PrimaryPhoto)
                        && !context.Results.ProductImages.Any(x => x.Value?.Equals(part.PrimaryPhoto, ComparisonType) == true)
                        && imagesAdded < ProcessingContext.MaxImagesPerSupplier && context.Results.ProductImages.Count < ProcessingContext.MaxImagesTotal)
                    {
                        context.Results.ProductImages.Add(new NameValuePair<string>(part.ManufacturerPartNumber, part.PrimaryPhoto));
                        imagesAdded++;
                    }

                    // if there is a datasheet that hasn't been added, add it
                    if (!string.IsNullOrEmpty(part.PrimaryDatasheet) && !context.Results.Datasheets.Any(x => x.Value?.DatasheetUrl?.Equals(part.PrimaryDatasheet, ComparisonType) == true))
                    {
                        var datasheetSource = new DatasheetSource($"https://{_configuration.ResourceSource}/{ProcessingContext.MissingDatasheetCoverName}", part.PrimaryDatasheet,
                            part.ManufacturerPartNumber, "", part.Manufacturer?.Value ?? string.Empty);
                        context.Results.Datasheets.Add(new NameValuePair<DatasheetSource>(part.ManufacturerPartNumber, datasheetSource));
                    }
                    context.Results.Parts.Add(new CommonPart
                    {
                        Rank = _resultsRank,
                        SwarmPartNumberManufacturerId = null,
                        Supplier = api.Name,
                        SupplierPartNumber = part.DigiKeyPartNumber,
                        BasePartNumber = basePart,
                        AdditionalPartNumbers = additionalPartNumbers,
                        Manufacturer = part.Manufacturer?.Value ?? string.Empty,
                        ManufacturerPartNumber = part.ManufacturerPartNumber,
                        Cost = (double)part.UnitPrice,
                        Currency = currency,
                        DatasheetUrls = new List<string> { part.PrimaryDatasheet ?? string.Empty },
                        Description = part.ProductDescription + Environment.NewLine + part.DetailedDescription,
                        ImageUrl = part.PrimaryPhoto,
                        PackageType = part.Parameters
                            ?.Where(x => x.Parameter.Equals("Package / Case", StringComparison.InvariantCultureIgnoreCase))
                            .Select(x => x.Value)
                            .FirstOrDefault(),
                        MountingTypeId = (int)mountingTypeId,
                        PartType = "",
                        ProductUrl = part.ProductUrl,
                        Status = part.ProductStatus,
                        QuantityAvailable = part.QuantityAvailable,
                        MinimumOrderQuantity = part.MinimumOrderQuantity,
                        //FactoryStockAvailable = factoryStockAvailable,
                        //FactoryLeadTime = part.LeadTime
                    });
                }
            }
            return Task.CompletedTask;
        }

        private async Task<IServiceResult<Product?>> GetBarcodeInfoProductAsync(IIntegrationApi api, string barcode, ScannedBarcodeType barcodeType)
        {
            if (!api.IsEnabled)
                return ServiceResult<Product?>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            // currently only supports DigiKey, as Mouser barcodes are part numbers
            var response = new PartResults();
            var digikeyResponse = new ProductBarcodeResponse();
            if (api.IsEnabled)
            {
                var apiResponse = await ((DigikeyApi)api).GetBarcodeDetailsAsync(barcode, barcodeType);
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<Product?>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
                else if (apiResponse.Errors?.Any() == true)
                {
                    //return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                    // try looking up the part by its barcode value, which could be a product search
                    digikeyResponse = new ProductBarcodeResponse
                    {
                        DigiKeyPartNumber = barcode
                    };
                }
                else
                {
                    digikeyResponse = (ProductBarcodeResponse?)apiResponse.Response;
                }

                if (digikeyResponse != null && !string.IsNullOrEmpty(digikeyResponse.DigiKeyPartNumber))
                {
                    var partResponse = await api.GetProductDetailsAsync(digikeyResponse.DigiKeyPartNumber);
                    if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                    {
                        var part = (Product?)partResponse.Response;
                        return ServiceResult<Product?>.Create(part);
                    }
                }
            }

            return ServiceResult<Product?>.Create(null);
        }
    }
}
