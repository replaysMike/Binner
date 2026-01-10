using ApiClient.Constants;
using Binner.Common.Integrations;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations;
using Binner.Model.Integrations.DigiKey;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using V3 = Binner.Model.Integrations.DigiKey.V3;
using V4 = Binner.Model.Integrations.DigiKey.V4;

namespace Binner.Services.Integrations.ResponseProcessors
{
    public class DigiKeyPartInfoResponseProcessor : IResponseProcessor
    {
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogger _logger;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly UserConfiguration _userConfiguration;
        private readonly int _resultsRank;
        private readonly int _maxResults;

        public DigiKeyPartInfoResponseProcessor(ILogger logger, WebHostServiceConfiguration configuration, UserConfiguration userConfiguration, int resultsRank, int maxResults = ApiConstants.MaxRecords)
        {
            _logger = logger;
            _configuration = configuration;
            _userConfiguration = userConfiguration;
            _resultsRank = resultsRank;
            _maxResults = maxResults;
        }

        public async Task ExecuteAsync(IIntegrationApi api, ProcessingContext context)
        {
            // fetch part info
            await FetchPartsAsync(api, context);
        }

        private async Task FetchPartsAsync(IIntegrationApi api, ProcessingContext context)
        {
            IApiResponse? apiResponse = null;
            try
            {
                apiResponse = await api.SearchAsync(context.PartNumber, context.PartType, context.MountingType, _maxResults);
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
                throw new ApiErrorException(apiResponse, "Api returned a fatal error.");
            }

            switch (apiResponse.Response)
            {
                case Binner.Model.Integrations.DigiKey.V3.KeywordSearchResponse v3Response:
                    await GetDigikeyV3ResponseAsync(api, context, apiResponse, v3Response);
                    break;
                case Binner.Model.Integrations.DigiKey.V4.KeywordSearchResponse v4Response:
                    await GetDigikeyV4ResponseAsync(api, context, apiResponse, v4Response);
                    break;
                default:
                    throw new NotImplementedException($"Type {apiResponse.Response?.GetType()} is not handled.");
            }
        }

        private async Task GetDigikeyV3ResponseAsync(IIntegrationApi api, ProcessingContext context, IApiResponse apiResponse, V3.KeywordSearchResponse digikeyResponse)
        {
            // if no part found, and it's numerical, try getting barcode info
            if (digikeyResponse.Products.Any() != true)
            {
                var isNumber = Regex.IsMatch(context.PartNumber, @"^\d+$");
                if (isNumber)
                {
                    var barcode = context.PartNumber;
                    IServiceResult<V3.Product?>? barcodeResult = null;
                    try
                    {
                        barcodeResult = await GetBarcodeInfoProductAsync(api, barcode, ScannedLabelType.Product);
                        digikeyResponse = new V3.KeywordSearchResponse();
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
                                    if (partResponse.Response is V3.Product)
                                    {
                                        var part = (V3.Product?)partResponse.Response;
                                        if (part != null)
                                            digikeyResponse.Products.Add(part);
                                    }
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
                        var part = (V3.Product?)partResponse.Response;
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

            await ToCommonPartAsync(api, digikeyResponse, context);
        }

        private async Task GetDigikeyV4ResponseAsync(IIntegrationApi api, ProcessingContext context, IApiResponse apiResponse, V4.KeywordSearchResponse digikeyResponse)
        {
            // if no part found, and it's numerical, try getting barcode info
            if (digikeyResponse.Products.Any() != true)
            {
                var isNumber = Regex.IsMatch(context.PartNumber, @"^\d+$");
                if (isNumber)
                {
                    var barcode = context.PartNumber;
                    IServiceResult<V3.Product?>? barcodeResult = null;
                    try
                    {
                        barcodeResult = await GetBarcodeInfoProductAsync(api, barcode, ScannedLabelType.Product);
                        digikeyResponse = new V4.KeywordSearchResponse();

                        // todo: map this to V4 product?
                        //if (barcodeResult?.Response != null)
                        //    digikeyResponse.Products.Add(barcodeResult.Response);
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
                                    var details = (V4.ProductDetails?)partResponse.Response;
                                    if (details != null)
                                    {
                                        var part = details.Product;
                                        if (part != null)
                                            digikeyResponse.Products.Add(part);
                                    }
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
                        var productDetails = (V4.ProductDetails?)partResponse.Response;
                        if (productDetails != null)
                        {
                            var part = productDetails.Product;
                            if (part != null)
                                digikeyResponse.Products.Add(part);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{nameof(DigikeyApi)}]: {ex.GetBaseException().Message}");
                    apiResponse.Errors.Add($"Error fetching product details on supplier part number '{WebUtility.HtmlEncode(context.SupplierPartNumbers)}': {ex.GetBaseException().Message}");

                }
            }

            context.ApiResponses[nameof(DigikeyApi)].IsSuccess = digikeyResponse.Products.Any();

            await ToCommonPartAsync(api, digikeyResponse, context);
        }

        private Task ToCommonPartAsync(IIntegrationApi api, V3.KeywordSearchResponse response, ProcessingContext context)
        {
            var imagesAdded = 0;
            if (!response.Products.Any()) return Task.CompletedTask;

            // add the exact matches as a higher rank
            foreach (var part in response.ExactDigiKeyProduct)
            {
                MapV3PartToResponse(api, response, part, context, _resultsRank, ref imagesAdded);
            }
            // add the exact matches as a higher rank
            foreach (var part in response.ExactManufacturerProducts)
            {
                MapV3PartToResponse(api, response, part, context, _resultsRank - 1, ref imagesAdded);
            }
            foreach (var part in response.Products)
            {
                MapV3PartToResponse(api, response, part, context, _resultsRank - 2, ref imagesAdded);
            }
            return Task.CompletedTask;
        }

        private void MapV3PartToResponse(IIntegrationApi api, V3.KeywordSearchResponse response, V3.Product part, ProcessingContext context, int rank, ref int imagesAdded)
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
                    currency = _userConfiguration.Currency.ToString().ToUpper();
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
                    Categories = new List<CommonCategory> { 
                        new CommonCategory { 
                            Name = part.Category?.Value ?? string.Empty, 
                            Description = string.Empty,
                            ChildCategories = new List<CommonCategory>
                            {
                                new CommonCategory
                                {
                                    Name = part.Family?.Value ?? string.Empty,
                                    Description = string.Empty
                                }
                            }
                        } 
                    },
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
                    FactoryLeadTime = part.LeadStatus,
                    Series = part.Series?.Value,
                    RohsStatus = part.RoHSStatus,
                    ReachStatus = part.TariffDescription,
                    // these values are not available in the V3 api
                    //ExportControlClassNumber = part.Classifications?.ExportControlClassNumber,
                    //HtsusCode = part.Classifications?.HtsusCode,
                    //MoistureSensitivityLevel = part.Classifications?.MoistureSensitivityLevel,
                    Parametrics = part.Parameters?.Select(x => new PartParametric
                    {
                        DigiKeyParameterId = x.ParameterId,
                        DigiKeyParameterText = x.Parameter,
                        DigiKeyParameterType = null,
                        DigiKeyValueId = x.ValueId,
                        DigiKeyValueText = x.Value,
                        Name = x.Parameter,
                        Value = x.Value ?? string.Empty,
                        ValueNumber = CommonPartValueFormatter.DetectValueFromText(x.Value),
                        Units = CommonPartValueFormatter.DetectUnitsFromText(x.Value),
                    }).ToList(),
                    // todo: source if there is an api endpoint for this
                    Models = new List<PartModel>()
                });
            }
        }

        private Task ToCommonPartAsync(IIntegrationApi api, V4.KeywordSearchResponse response, ProcessingContext context)
        {
            var imagesAdded = 0;
            if (!response.Products.Any()) return Task.CompletedTask;

            // add the exact matches as a higher rank
            foreach (var part in response.ExactMatches)
                MapV4PartToResponse(api, response, part, context, _resultsRank, ref imagesAdded);

            // add all matches that aren't already added
            foreach (var part in response.Products)
            {
                MapV4PartToResponse(api, response, part, context, _resultsRank + 1, ref imagesAdded);
            }
            return Task.CompletedTask;
        }

        private void MapV4PartToResponse(IIntegrationApi api, V4.KeywordSearchResponse response, V4.Product part, ProcessingContext context, int rank, ref int imagesAdded)
        {
            var additionalPartNumbers = new List<string>();
            if (part.OtherNames?.Any() == true)
                additionalPartNumbers.AddRange(part.OtherNames);
            if (part.Parameters != null)
            {
                var basePart = part.Parameters
                    // todo: do we need to ask for it?
                    .Where(x => x.ParameterText.Equals("Base Part Number", ComparisonType))
                    .Where(x => x.ParameterText.Equals("Utilized IC / Part", ComparisonType))
                    .Select(x => x.ValueText)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(basePart))
                    additionalPartNumbers.Add(basePart);
                else
                {
                    if (part.ManufacturerProductNumber?.Contains(context.PartNumber, ComparisonType) == true)
                        basePart = context.PartNumber;
                }

                if (string.IsNullOrEmpty(basePart))
                    basePart = part.ManufacturerProductNumber;
                var mountingTypeId = MountingType.None;
                var mountingTypeParameter = part.Parameters
                    .Where(x => x.ParameterText.Equals("Mounting Type", ComparisonType))
                    .Select(x => x.ValueText?.Replace(" ", ""))
                    .FirstOrDefault();

                if (mountingTypeParameter?.Contains(",") == true)
                {
                    // DigiKey very rarely returns a part as being more than one mounting type. Pick the last one.
                    mountingTypeParameter = mountingTypeParameter.Split(",", StringSplitOptions.RemoveEmptyEntries).Last();
                }

                Enum.TryParse<MountingType>(mountingTypeParameter, out mountingTypeId);
                var currency = response.SearchLocaleUsed.Currency;
                if (string.IsNullOrEmpty(currency))
                    currency = _userConfiguration.Currency.ToString().ToUpper();
                var packageType = part.Parameters
                    ?.Where(x => x.ParameterText.Equals("Supplier Device Package", ComparisonType))
                    .Select(x => x.ValueText)
                    .FirstOrDefault();
                if (string.IsNullOrEmpty(packageType))
                    packageType = part.Parameters
                        ?.Where(x => x.ParameterText.Equals("Package / Case", ComparisonType))
                        .Select(x => x.ValueText)
                        .FirstOrDefault();
                if (!string.IsNullOrEmpty(part.PhotoUrl)
                    && !context.Results.ProductImages.Any(x => x.Value?.Equals(part.PhotoUrl, ComparisonType) == true)
                    && imagesAdded < ProcessingContext.MaxImagesPerSupplier && context.Results.ProductImages.Count < ProcessingContext.MaxImagesTotal)
                {
                    context.Results.ProductImages.Add(new NameValuePair<string>(part.ManufacturerProductNumber ?? string.Empty, part.PhotoUrl));
                    imagesAdded++;
                }

                // if there is a datasheet that hasn't been added, add it
                if (!string.IsNullOrEmpty(part.DatasheetUrl) && !context.Results.Datasheets.Any(x => x.Value?.DatasheetUrl?.Equals(part.DatasheetUrl, ComparisonType) == true))
                {
                    var datasheetSource = new DatasheetSource($"https://{_configuration.ResourceSource}/{ProcessingContext.MissingDatasheetCoverName}", part.DatasheetUrl,
                        part.ManufacturerProductNumber, "", part.Manufacturer?.Name ?? string.Empty);
                    context.Results.Datasheets.Add(new NameValuePair<DatasheetSource>(part.ManufacturerProductNumber ?? string.Empty, datasheetSource));
                }

                var minimumOrderQuantity = part.ProductVariations.Where(x => x.MinimumOrderQuantity > 0).Select(x => x.MinimumOrderQuantity).FirstOrDefault();
                if (minimumOrderQuantity == 0)
                    minimumOrderQuantity = part.ProductVariations.Select(x => x.MinimumOrderQuantity).FirstOrDefault();
                var factoryStockAvailable = part.ProductVariations.Where(x => x.MinimumOrderQuantity > 0).Select(x => x.QuantityAvailableforPackageType).FirstOrDefault();
                if (factoryStockAvailable == 0)
                    factoryStockAvailable = part.ProductVariations.Select(x => x.QuantityAvailableforPackageType).FirstOrDefault();
                context.Results.Parts.Add(new CommonPart
                {
                    Rank = rank,
                    SwarmPartNumberManufacturerId = null,
                    Supplier = api.Name,
                    SupplierPartNumber = part.ProductVariations.Where(x => x.DigiKeyProductNumber != null).Select(x => x.DigiKeyProductNumber).FirstOrDefault() ?? string.Empty,
                    BasePartNumber = basePart,
                    AdditionalPartNumbers = additionalPartNumbers,
                    Manufacturer = part.Manufacturer?.Name ?? string.Empty,
                    ManufacturerPartNumber = part.ManufacturerProductNumber ?? string.Empty,
                    Categories = RecursiveMapCategories(part),
                    Cost = (double)part.UnitPrice,
                    Currency = currency,
                    DatasheetUrls = new List<string> { part.DatasheetUrl ?? string.Empty },
                    Description = part.Description.ProductDescription + Environment.NewLine + part.Description.DetailedDescription,
                    ImageUrl = part.PhotoUrl,
                    PackageType = part.Parameters
                        ?.Where(x => x.ParameterText.Equals("Package / Case", StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => x.ValueText)
                        .FirstOrDefault(),
                    MountingTypeId = (int)mountingTypeId,
                    PartType = "",
                    ProductUrl = part.ProductUrl,
                    Status = part.ProductStatus.Status,
                    QuantityAvailable = part.QuantityAvailable,
                    MinimumOrderQuantity = minimumOrderQuantity,
                    FactoryStockAvailable = part.ManufacturerPublicQuantity,
                    FactoryLeadTime = part.ManufacturerLeadWeeks,
                    Series = part.Series?.Name,
                    RohsStatus = part.Classifications?.RohsStatus,
                    ReachStatus = part.Classifications?.ReachStatus,
                    ExportControlClassNumber = part.Classifications?.ExportControlClassNumber,
                    HtsusCode = part.Classifications?.HtsusCode,
                    MoistureSensitivityLevel = part.Classifications?.MoistureSensitivityLevel,
                    Parametrics = part.Parameters?.Select(x => new PartParametric
                    {
                        DigiKeyParameterId = x.ParameterId,
                        DigiKeyParameterType = x.ParameterType.ToString(),
                        DigiKeyParameterText = x.ParameterText,
                        DigiKeyValueId = x.ValueId,
                        DigiKeyValueText = x.ValueText,
                        Name = x.ParameterText,
                        Value = x.ValueText,
                        ValueNumber = CommonPartValueFormatter.DetectValueFromText(x.ValueText),
                        Units = CommonPartValueFormatter.DetectUnitsFromText(x.ValueText),
                    }).ToList(),
                    // todo: source if there is an api endpoint for this
                    Models = new List<PartModel>()
                });
            }
        }

        private List<CommonCategory> RecursiveMapCategories(V4.Product part)
        {
            var categories = DoRecursive(part.Category);
            return categories;
        }

        private List<CommonCategory> DoRecursive(V4.CategoryNode categoryNode)
        {
            var categories = new List<CommonCategory>();
            var category = new CommonCategory
            {
                Name = categoryNode.Name,
                Description = categoryNode.SeoDescription
            };
            if (categoryNode.ChildCategories.Any())
            {
                foreach(var node in categoryNode.ChildCategories)
                {
                    var childCategories = DoRecursive(node);
                    if (childCategories.Any())
                    {
                        foreach(var childCategory in childCategories)
                            category.ChildCategories.Add(childCategory);
                    }
                }
            }
            categories.Add(category);
            return categories;
        }

        private async Task<IServiceResult<V3.Product?>> GetBarcodeInfoProductAsync(IIntegrationApi api, string barcode, ScannedLabelType barcodeType)
        {
            if (!api.IsEnabled)
                return ServiceResult<V3.Product?>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            // currently only supports DigiKey, as Mouser barcodes are part numbers
            var response = new PartResults();
            var digikeyResponse = new ProductBarcodeResponse();
            if (api.IsEnabled)
            {
                var apiResponse = await ((DigikeyApi)api).GetBarcodeDetailsAsync(barcode, barcodeType);
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<V3.Product?>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
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
                        var part = (V3.Product?)partResponse.Response;
                        return ServiceResult<V3.Product?>.Create(part);
                    }
                }
            }

            return ServiceResult<V3.Product?>.Create(null);
        }
    }
}
