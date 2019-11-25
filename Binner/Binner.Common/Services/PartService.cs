using Binner.Common.Integrations;
using Binner.Common.Integrations.Models.Digikey;
using Binner.Common.Integrations.Models.Mouser;
using Binner.Common.Models;
using Binner.Common.Models.Responses;
using Binner.Common.StorageProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class PartService : IPartService
    {
        private IStorageProvider _storageProvider;
        private OctopartApi _octopartApi;
        private DigikeyApi _digikeyApi;
        private MouserApi _mouserApi;
        private RequestContextAccessor _requestContext;

        public PartService(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor, OctopartApi octopartApi, DigikeyApi digikeyApi, MouserApi mouserApi)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _octopartApi = octopartApi;
            _digikeyApi = digikeyApi;
            _mouserApi = mouserApi;
        }

        public async Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords)
        {
            return await _storageProvider.FindPartsAsync(keywords, _requestContext.GetUserContext());
        }

        public async Task<long> GetPartsCountAsync()
        {
            return await _storageProvider.GetPartsCountAsync(_requestContext.GetUserContext());
        }

        public async Task<decimal> GetPartsValueAsync()
        {
            return await _storageProvider.GetPartsValueAsync(_requestContext.GetUserContext());
        }

        public async Task<ICollection<Part>> GetLowStockAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetLowStockAsync(request, _requestContext.GetUserContext());
        }

        public async Task<Part> GetPartAsync(string partNumber)
        {
            return await _storageProvider.GetPartAsync(partNumber, _requestContext.GetUserContext());
        }

        public async Task<ICollection<Part>> GetPartsAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetPartsAsync(request, _requestContext.GetUserContext());
        }

        public async Task<ICollection<Part>> GetPartsAsync(Expression<Func<Part, bool>> condition)
        {
            return await _storageProvider.GetPartsAsync(condition, _requestContext.GetUserContext());
        }

        public async Task<Part> AddPartAsync(Part part)
        {
            return await _storageProvider.AddPartAsync(part, _requestContext.GetUserContext());
        }

        public async Task<Part> UpdatePartAsync(Part part)
        {
            return await _storageProvider.UpdatePartAsync(part, _requestContext.GetUserContext());
        }

        public async Task<bool> DeletePartAsync(Part part)
        {
            return await _storageProvider.DeletePartAsync(part, _requestContext.GetUserContext());
        }

        public async Task<PartType> GetOrCreatePartTypeAsync(PartType partType)
        {
            if (partType == null) throw new ArgumentNullException(nameof(partType));
            if (partType.Name == null) throw new ArgumentNullException(nameof(partType.Name));
            return await _storageProvider.GetOrCreatePartTypeAsync(partType, _requestContext.GetUserContext());
        }

        public async Task<PartType> GetPartTypeAsync(int partTypeId)
        {
            return await _storageProvider.GetPartTypeAsync(partTypeId, _requestContext.GetUserContext());
        }

        public async Task<ICollection<PartType>> GetPartTypesAsync()
        {
            return await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
        }

        /// <summary>
        /// Get an external order
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public async Task<IServiceResult<ExternalOrderResponse>> GetExternalOrderAsync(string orderId, string supplier)
        {
            switch (supplier.ToLower())
            {
                case "digikey":
                    if (!_digikeyApi.IsConfigured) throw new InvalidOperationException($"DigiKey Api is not configured!");
                    return await GetExternalDigikeyOrderAsync(orderId);
                case "mouser":
                    if (!_mouserApi.IsConfigured) throw new InvalidOperationException($"Mouser Api is not configured!");
                    return await GetExternalMouserOrderAsync(orderId);
                default:
                    throw new InvalidOperationException($"Unknown supplier {supplier}");
            }
        }

        private async Task<IServiceResult<ExternalOrderResponse>> GetExternalDigikeyOrderAsync(string orderId)
        {
            var apiResponse = await _digikeyApi.GetOrderAsync(orderId);
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<ExternalOrderResponse>.Create(true, apiResponse.RedirectUrl, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<ExternalOrderResponse>.Create(apiResponse.Errors, apiResponse.ApiName);
            var digikeyResponse = (OrderSearchResponse)apiResponse.Response;

            var lineItems = digikeyResponse.LineItems;
            var commonParts = new List<CommonPart>();
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            // look up every part by digikey part number
            foreach (var lineItem in lineItems)
            {
                // get details on this digikey part
                var partResponse = await _digikeyApi.GetProductDetailsAsync(lineItem.DigiKeyPartNumber);
                if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                {
                    var part = (Product)partResponse.Response;
                    // convert the part to a common part
                    var additionalPartNumbers = new List<string>();
                    var basePart = part.Parameters.Where(x => x.Parameter.Equals("Base Part Number")).Select(x => x.Value).FirstOrDefault();
                    if (!string.IsNullOrEmpty(basePart))
                        additionalPartNumbers.Add(basePart);
                    var mountingTypeId = MountingType.ThroughHole;
                    Enum.TryParse<MountingType>(part.Parameters.Where(x => x.Parameter.Equals("Mounting Type")).Select(x => x.Value.Replace(" ", "")).FirstOrDefault(), out mountingTypeId);
                    var currency = digikeyResponse.Currency;
                    if (string.IsNullOrEmpty(currency))
                        currency = "USD";
                    var packageType = part.Parameters
                            ?.Where(x => x.Parameter.Equals("Supplier Device Package", StringComparison.InvariantCultureIgnoreCase))
                            .Select(x => x.Value)
                            .FirstOrDefault();
                    if (string.IsNullOrEmpty(packageType))
                        packageType = part.Parameters
                            ?.Where(x => x.Parameter.Equals("Package / Case", StringComparison.InvariantCultureIgnoreCase))
                            .Select(x => x.Value)
                            .FirstOrDefault();
                    commonParts.Add(new CommonPart
                    {
                        SupplierPartNumber = part.DigiKeyPartNumber,
                        Supplier = "DigiKey",
                        ManufacturerPartNumber = part.ManufacturerPartNumber,
                        Manufacturer = part.Manufacturer.Value,
                        Description = part.ProductDescription + "\r\n" + part.DetailedDescription,
                        ImageUrl = part.PrimaryPhoto,
                        DatasheetUrls = new List<string> { part.PrimaryDatasheet },
                        ProductUrl = part.ProductUrl,
                        Status = part.ProductStatus,
                        Currency = currency,
                        AdditionalPartNumbers = additionalPartNumbers,
                        BasePartNumber = basePart,
                        MountingTypeId = (int)mountingTypeId,
                        PackageType = packageType,
                        Cost = lineItem.UnitPrice,
                        Quantity = lineItem.Quantity,
                        Reference = lineItem.CustomerReference,
                    });
                }
            }
            foreach (var part in commonParts)
            {
                part.PartType = DeterminePartType(part, partTypes);
                part.Keywords = DetermineKeywords(part, partTypes);
            }
            return ServiceResult<ExternalOrderResponse>.Create(new ExternalOrderResponse
            {
                OrderDate = digikeyResponse.ShippingDetails.Any() ? DateTime.Parse(digikeyResponse.ShippingDetails.First().DateTransaction ?? DateTime.MinValue.ToString()) : DateTime.MinValue,
                Currency = digikeyResponse.Currency,
                CustomerId = digikeyResponse.CustomerId.ToString(),
                Amount = lineItems.Sum(x => x.TotalPrice),
                TrackingNumber = digikeyResponse.ShippingDetails.Any() ? digikeyResponse.ShippingDetails.First().TrackingUrl : "",
                Parts = commonParts
            });
        }

        private async Task<IServiceResult<ExternalOrderResponse>> GetExternalMouserOrderAsync(string orderId)
        {
            var apiResponse = await _mouserApi.GetOrderAsync(orderId);
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<ExternalOrderResponse>.Create(true, apiResponse.RedirectUrl, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<ExternalOrderResponse>.Create(apiResponse.Errors, apiResponse.ApiName);
            var mouserOrderResponse = (Order)apiResponse.Response;


            var lineItems = mouserOrderResponse.OrderLines;
            var commonParts = new List<CommonPart>();
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            // look up every part by digikey part number
            foreach (var lineItem in lineItems)
            {
                // get details on this digikey part
                var partResponse = await _mouserApi.GetProductDetailsAsync(lineItem.MouserPartNumber);
                if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                {
                    var searchResults = (ICollection<MouserPart>)partResponse.Response;
                    // convert the part to a common part
                    var part = searchResults.First();
                    commonParts.Add(new CommonPart
                    {
                        SupplierPartNumber = part.MouserPartNumber,
                        Supplier = "Mouser",
                        ManufacturerPartNumber = part.ManufacturerPartNumber,
                        Manufacturer = part.Manufacturer,
                        Description = part.Description,
                        ImageUrl = part.ImagePath,
                        DatasheetUrls = new List<string> { part.DataSheetUrl },
                        ProductUrl = part.ProductDetailUrl,
                        Status = part.LifecycleStatus,
                        Currency = mouserOrderResponse.CurrencyCode,
                        AdditionalPartNumbers = new List<string>(),
                        BasePartNumber = "",
                        MountingTypeId = 0,
                        PackageType = "",
                        Cost = lineItem.UnitPrice,
                        Quantity = lineItem.Quantity,
                        Reference = lineItem.CartItemCustPartNumber,
                    });
                }
            }
            foreach (var part in commonParts)
            {
                part.PartType = DeterminePartType(part, partTypes);
                part.Keywords = DetermineKeywords(part, partTypes);
            }
            return ServiceResult<ExternalOrderResponse>.Create(new ExternalOrderResponse
            {
                OrderDate = DateTime.MinValue,
                Currency = mouserOrderResponse.CurrencyCode,
                CustomerId = "",
                Amount = mouserOrderResponse.OrderTotal,
                TrackingNumber = "",
                Parts = commonParts
            });
        }

        public async Task<IServiceResult<PartResults>> GetBarcodeInfoAsync(string barcode)
        {
            // currently only supports DigiKey, as Mouser barcodes are part numbers
            var response = new PartResults();
            var digikeyResponse = new Integrations.Models.Digikey.ProductBarcodeResponse();
            if (_digikeyApi.IsConfigured)
            {
                var apiResponse = await _digikeyApi.GetBarcodeDetailsAsync(barcode);
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults>.Create(true, apiResponse.RedirectUrl, apiResponse.Errors, apiResponse.ApiName);
                else if (apiResponse.Errors?.Any() == true)
                    return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                digikeyResponse = (ProductBarcodeResponse)apiResponse.Response;
                if (digikeyResponse != null)
                {
                    var partResponse = await _digikeyApi.GetProductDetailsAsync(digikeyResponse.DigiKeyPartNumber);
                    if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                    {
                        var part = (Product)partResponse.Response;
                        var additionalPartNumbers = new List<string>();
                        var basePart = part.Parameters.Where(x => x.Parameter.Equals("Base Part Number")).Select(x => x.Value).FirstOrDefault();
                        if (!string.IsNullOrEmpty(basePart))
                            additionalPartNumbers.Add(basePart);
                        var mountingTypeId = MountingType.ThroughHole;
                        Enum.TryParse<MountingType>(part.Parameters.Where(x => x.Parameter.Equals("Mounting Type")).Select(x => x.Value.Replace(" ", "")).FirstOrDefault(), out mountingTypeId);
                        var currency = string.Empty;
                        if (string.IsNullOrEmpty(currency))
                            currency = "USD";
                        var packageType = part.Parameters
                                ?.Where(x => x.Parameter.Equals("Supplier Device Package", StringComparison.InvariantCultureIgnoreCase))
                                .Select(x => x.Value)
                                .FirstOrDefault();
                        if (string.IsNullOrEmpty(packageType))
                            packageType = part.Parameters
                                ?.Where(x => x.Parameter.Equals("Package / Case", StringComparison.InvariantCultureIgnoreCase))
                                .Select(x => x.Value)
                                .FirstOrDefault();
                        response.Parts.Add(new CommonPart
                        {
                            Supplier = "DigiKey",
                            SupplierPartNumber = part.DigiKeyPartNumber,
                            BasePartNumber = basePart,
                            AdditionalPartNumbers = additionalPartNumbers,
                            Manufacturer = part.Manufacturer.Value,
                            ManufacturerPartNumber = part.ManufacturerPartNumber,
                            Cost = part.UnitPrice,
                            Currency = currency,
                            DatasheetUrls = new List<string> { part.PrimaryDatasheet },
                            Description = part.ProductDescription + "\r\n" + part.DetailedDescription,
                            ImageUrl = part.PrimaryPhoto,
                            PackageType = part.Parameters
                                ?.Where(x => x.Parameter.Equals("Package / Case", StringComparison.InvariantCultureIgnoreCase))
                                .Select(x => x.Value)
                                .FirstOrDefault(),
                            MountingTypeId = (int)mountingTypeId,
                            PartType = "",
                            ProductUrl = part.ProductUrl,
                            Status = part.ProductStatus,
                            Quantity = digikeyResponse.Quantity
                        });
                    }
                    else
                    {
                        response.Parts.Add(new CommonPart
                        {
                            Supplier = "DigiKey",
                            SupplierPartNumber = digikeyResponse.DigiKeyPartNumber,
                            Manufacturer = digikeyResponse.ManufacturerName,
                            ManufacturerPartNumber = digikeyResponse.ManufacturerPartNumber,
                            Description = digikeyResponse.ProductDescription,
                            Quantity = digikeyResponse.Quantity
                        });
                    }
                }
            }

            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            foreach (var part in response.Parts)
            {
                part.PartType = DeterminePartType(part, partTypes);
                part.Keywords = DetermineKeywords(part, partTypes);
            }

            return ServiceResult<PartResults>.Create(response);
        }

        public async Task<IServiceResult<PartResults>> GetPartInformationAsync(string partNumber, string partType = "", string mountingType = "")
        {
            var datasheets = new List<string>();
            var response = new PartResults();
            var digikeyResponse = new Integrations.Models.Digikey.KeywordSearchResponse();
            var searchKeywords = partNumber;
            ICollection<Integrations.Models.Mouser.MouserPart> mouserParts = new List<Integrations.Models.Mouser.MouserPart>();
            if (_octopartApi.IsConfigured)
            {
                var octopartResponse = await _octopartApi.GetDatasheetsAsync(partNumber);
                datasheets.AddRange((ICollection<string>)octopartResponse.Response);
            }
            if (_digikeyApi.IsConfigured)
            {
                var apiResponse = await _digikeyApi.SearchAsync(searchKeywords, partType, mountingType);
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults>.Create(true, apiResponse.RedirectUrl, apiResponse.Errors, apiResponse.ApiName);
                else if (apiResponse.Errors?.Any() == true)
                    return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                digikeyResponse = (KeywordSearchResponse)apiResponse.Response;
            }
            if (_mouserApi.IsConfigured)
            {
                var apiResponse = await _mouserApi.SearchAsync(searchKeywords, partType, mountingType);
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults>.Create(true, apiResponse.RedirectUrl, apiResponse.Errors, apiResponse.ApiName);
                else if (apiResponse.Errors?.Any() == true)
                    return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                mouserParts = (ICollection<MouserPart>)apiResponse.Response;
            }

            foreach (var part in digikeyResponse.Products)
            {
                var additionalPartNumbers = new List<string>();
                var basePart = part.Parameters.Where(x => x.Parameter.Equals("Base Part Number")).Select(x => x.Value).FirstOrDefault();
                if (!string.IsNullOrEmpty(basePart))
                    additionalPartNumbers.Add(basePart);
                var mountingTypeId = MountingType.ThroughHole;
                Enum.TryParse<MountingType>(part.Parameters.Where(x => x.Parameter.Equals("Mounting Type")).Select(x => x.Value.Replace(" ", "")).FirstOrDefault(), out mountingTypeId);
                var currency = digikeyResponse.SearchLocaleUsed.Currency;
                if (string.IsNullOrEmpty(currency))
                    currency = "USD";
                var packageType = part.Parameters
                        ?.Where(x => x.Parameter.Equals("Supplier Device Package", StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => x.Value)
                        .FirstOrDefault();
                if (string.IsNullOrEmpty(packageType))
                    packageType = part.Parameters
                        ?.Where(x => x.Parameter.Equals("Package / Case", StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => x.Value)
                        .FirstOrDefault();
                response.Parts.Add(new CommonPart
                {
                    Supplier = "DigiKey",
                    SupplierPartNumber = part.DigiKeyPartNumber,
                    BasePartNumber = basePart,
                    AdditionalPartNumbers = additionalPartNumbers,
                    Manufacturer = part.Manufacturer.Value,
                    ManufacturerPartNumber = part.ManufacturerPartNumber,
                    Cost = part.UnitPrice,
                    Currency = currency,
                    DatasheetUrls = new List<string> { part.PrimaryDatasheet },
                    Description = part.ProductDescription + "\r\n" + part.DetailedDescription,
                    ImageUrl = part.PrimaryPhoto,
                    PackageType = part.Parameters
                        ?.Where(x => x.Parameter.Equals("Package / Case", StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => x.Value)
                        .FirstOrDefault(),
                    MountingTypeId = (int)mountingTypeId,
                    PartType = "",
                    ProductUrl = part.ProductUrl,
                    Status = part.ProductStatus
                });
            }

            foreach (var part in mouserParts)
            {
                var mountingTypeId = MountingType.ThroughHole;
                var currency = part.PriceBreaks?.OrderBy(x => x.Quantity).FirstOrDefault()?.Currency;
                if (string.IsNullOrEmpty(currency))
                    currency = "USD";
                response.Parts.Add(new CommonPart
                {
                    Supplier = "Mouser",
                    SupplierPartNumber = part.MouserPartNumber,
                    BasePartNumber = "",
                    Manufacturer = part.Manufacturer,
                    ManufacturerPartNumber = part.ManufacturerPartNumber,
                    Cost = part.PriceBreaks?.OrderBy(x => x.Quantity).FirstOrDefault()?.Cost ?? 0,
                    Currency = currency,
                    DatasheetUrls = new List<string> { part.DataSheetUrl },
                    Description = part.Description,
                    ImageUrl = part.ImagePath,
                    PackageType = "",
                    MountingTypeId = (int)mountingTypeId,
                    PartType = "",
                    ProductUrl = part.ProductDetailUrl,
                    Status = part.LifecycleStatus
                });
            }

            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            foreach (var part in response.Parts)
            {
                part.PartType = DeterminePartType(part, partTypes);
                part.Keywords = DetermineKeywords(part, partTypes);
            }

            return ServiceResult<PartResults>.Create(response);
        }

        private ICollection<string> DetermineKeywords(CommonPart part, ICollection<PartType> partTypes)
        {
            // part type
            // important parts from description
            // alternate series numbers etc
            var keywords = new List<string>();
            var possiblePartTypes = GetMatchingPartTypes(part, partTypes);
            foreach (var possiblePartType in possiblePartTypes)
                if (!keywords.Contains(possiblePartType.Key.Name, StringComparer.InvariantCultureIgnoreCase))
                    keywords.Add(possiblePartType.Key.Name.ToLower());

            if (!keywords.Contains(part.ManufacturerPartNumber, StringComparer.InvariantCultureIgnoreCase))
                keywords.Add(part.ManufacturerPartNumber.ToLower());
            var desc = part.Description.ToLower().Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            // add the first 4 words of desc
            var wordCount = 0;
            var ignoredWords = new string[] { "and", "the", "in", "or", "in", "a", };
            foreach (var word in desc)
            {
                if (!ignoredWords.Contains(word, StringComparer.InvariantCultureIgnoreCase) && !keywords.Contains(word, StringComparer.InvariantCultureIgnoreCase))
                {
                    keywords.Add(word.ToLower());
                    wordCount++;
                }
                if (wordCount >= 4)
                    break;
            }
            foreach (var basePart in part.AdditionalPartNumbers)
                if (basePart != null && !keywords.Contains(basePart, StringComparer.InvariantCultureIgnoreCase))
                    keywords.Add(basePart.ToLower());
            var mountingType = (MountingType)part.MountingTypeId;
            if (!string.IsNullOrEmpty(mountingType.ToString()) && !keywords.Contains(mountingType.ToString(), StringComparer.InvariantCultureIgnoreCase))
                keywords.Add(mountingType.ToString().ToLower());

            return keywords.Distinct().ToList();
        }

        private IDictionary<PartType, int> GetMatchingPartTypes(PartMetadata metadata, ICollection<PartType> partTypes)
        {
            // load all part types
            var possiblePartTypes = new Dictionary<PartType, int>();
            foreach (var partType in partTypes)
            {
                if (string.IsNullOrEmpty(partType.Name))
                    continue;
                var addPart = false;
                if (metadata.Description?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                if (metadata.DetailedDescription?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                if (metadata.PartNumber?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                if (metadata.DatasheetUrl?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                if (addPart)
                {
                    if (possiblePartTypes.ContainsKey(partType))
                        possiblePartTypes[partType]++;
                    else
                        possiblePartTypes.Add(partType, 1);
                }

            }
            return possiblePartTypes;
        }

        private IDictionary<PartType, int> GetMatchingPartTypes(CommonPart part, ICollection<PartType> partTypes)
        {
            // load all part types
            var possiblePartTypes = new Dictionary<PartType, int>();
            foreach (var partType in partTypes)
            {
                if (string.IsNullOrEmpty(partType.Name))
                    continue;
                var addPart = false;
                if (part.Description?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                if (part.ManufacturerPartNumber?.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    addPart = true;
                foreach (var datasheet in part.DatasheetUrls)
                    if (datasheet.IndexOf(partType.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        addPart = true;
                if (addPart)
                {
                    if (possiblePartTypes.ContainsKey(partType))
                        possiblePartTypes[partType]++;
                    else
                        possiblePartTypes.Add(partType, 1);
                }

            }
            return possiblePartTypes;
        }

        private string DeterminePartType(CommonPart part, ICollection<PartType> partTypes)
        {
            var possiblePartTypes = GetMatchingPartTypes(part, partTypes);
            return possiblePartTypes
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key?.Name)
                .FirstOrDefault();
        }
    }
}
