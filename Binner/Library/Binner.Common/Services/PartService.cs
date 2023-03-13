using AutoMapper;
using Binner.Common.Integrations;
using Binner.Common.Integrations.Models.DigiKey;
using Binner.Common.Integrations.Models.Mouser;
using Binner.Common.Models;
using Binner.Common.Models.Configuration.Integrations;
using Binner.Common.Models.Responses;
using Binner.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Binner.Common.Integrations.Models.Arrow;
using Part = Binner.Model.Common.Part;

namespace Binner.Common.Services
{
    public class PartService : IPartService
    {
        private const string MissingDatasheetCoverName = "datasheetcover.png";
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly IStorageProvider _storageProvider;
        private readonly IMapper _mapper;
        private readonly IIntegrationApiFactory _integrationApiFactory;
        private readonly RequestContextAccessor _requestContext;
        private readonly ISwarmService _swarmService;

        public PartService(IStorageProvider storageProvider, IMapper mapper, IIntegrationApiFactory integrationApiFactory, ISwarmService swarmService, RequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _mapper = mapper;
            _integrationApiFactory = integrationApiFactory;
            _requestContext = requestContextAccessor;
            _swarmService = swarmService;
        }

        public async Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords)
        {
            return await _storageProvider.FindPartsAsync(keywords, _requestContext.GetUserContext());
        }

        public async Task<long> GetPartsCountAsync()
        {
            return await _storageProvider.GetPartsCountAsync(_requestContext.GetUserContext());
        }

        public async Task<long> GetUniquePartsCountAsync()
        {
            return await _storageProvider.GetUniquePartsCountAsync(_requestContext.GetUserContext());
        }

        public long GetUniquePartsMax()
        {
            return 0; // unlimited parts
        }

        public async Task<decimal> GetPartsValueAsync()
        {
            return await _storageProvider.GetPartsValueAsync(_requestContext.GetUserContext());
        }

        public async Task<PaginatedResponse<Part>> GetLowStockAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetLowStockAsync(request, _requestContext.GetUserContext());
        }

        public async Task<Part?> GetPartAsync(string partNumber)
        {
            return await _storageProvider.GetPartAsync(partNumber, _requestContext.GetUserContext());
        }

        public async Task<(Part? Part, ICollection<StoredFile> StoredFiles)> GetPartWithStoredFilesAsync(string partNumber)
        {
            var userContext = _requestContext.GetUserContext();
            var partEntity = await _storageProvider.GetPartAsync(partNumber, userContext);
            var storedFiles = new List<StoredFile>();
            if (partEntity != null)
            {
                var files = await _storageProvider.GetStoredFilesAsync(partEntity.PartId, null, userContext);
                if (files.Any())
                    storedFiles.AddRange(files.OrderByDescending(x => x.DateCreatedUtc));
            }
            return (partEntity, storedFiles);
        }

        public async Task<PaginatedResponse<Part>> GetPartsAsync(PaginatedRequest request)
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

        public async Task<PartType?> GetOrCreatePartTypeAsync(PartType partType)
        {
            if (partType == null) throw new ArgumentNullException(nameof(partType));
            if (partType.Name == null) throw new ArgumentNullException(nameof(partType.Name));
            return await _storageProvider.GetOrCreatePartTypeAsync(partType, _requestContext.GetUserContext());
        }

        public async Task<PartType?> GetPartTypeAsync(int partTypeId)
        {
            return await _storageProvider.GetPartTypeAsync(partTypeId, _requestContext.GetUserContext());
        }

        public async Task<ICollection<PartType>> GetPartTypesAsync()
        {
            return await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
        }

        public async Task<IServiceResult<CategoriesResponse?>> GetCategoriesAsync()
        {
            var user = _requestContext.GetUserContext();
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
            if (!digikeyApi.IsEnabled)
                return ServiceResult<CategoriesResponse>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            var apiResponse = await digikeyApi.GetCategoriesAsync();
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<CategoriesResponse>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<CategoriesResponse>.Create(apiResponse.Errors, apiResponse.ApiName);

            if (apiResponse.Response != null)
            {
                var digikeyResponse = (CategoriesResponse)apiResponse.Response;
                return ServiceResult<CategoriesResponse>.Create(digikeyResponse);
            }

            return ServiceResult<CategoriesResponse>.Create("Invalid response received", apiResponse.ApiName);
        }

        /// <summary>
        /// Get an external order
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public async Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(string orderId, string supplier)
        {
            switch (supplier.ToLower())
            {
                case "digikey":
                    return await GetExternalDigiKeyOrderAsync(orderId);
                case "mouser":
                    return await GetExternalMouserOrderAsync(orderId);
                default:
                    throw new InvalidOperationException($"Unknown supplier {supplier}");
            }
        }

        private async Task<IServiceResult<ExternalOrderResponse?>> GetExternalDigiKeyOrderAsync(string orderId)
        {
            var user = _requestContext.GetUserContext();
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
            if (!digikeyApi.IsEnabled)
                return ServiceResult<ExternalOrderResponse>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            var apiResponse = await digikeyApi.GetOrderAsync(orderId);
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<ExternalOrderResponse>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<ExternalOrderResponse>.Create(apiResponse.Errors, apiResponse.ApiName);
            var digikeyResponse = (OrderSearchResponse?)apiResponse.Response ?? new OrderSearchResponse();

            var lineItems = digikeyResponse.LineItems;
            var commonParts = new List<CommonPart>();
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            // look up every part by digikey part number
            foreach (var lineItem in lineItems)
            {
                // get details on this digikey part
                if (string.IsNullOrEmpty(lineItem.DigiKeyPartNumber))
                    continue;
                var partResponse = await digikeyApi.GetProductDetailsAsync(lineItem.DigiKeyPartNumber);
                if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                {
                    var part = (Product?)partResponse.Response ?? new Product();
                    // convert the part to a common part
                    var additionalPartNumbers = new List<string>();
                    var basePart = part.Parameters
                        .Where(x => x.Parameter.Equals("Base Part Number", ComparisonType))
                        .Select(x => x.Value)
                        .FirstOrDefault();
                    
                    if (!string.IsNullOrEmpty(basePart))
                        additionalPartNumbers.Add(basePart);
                    
                    if (string.IsNullOrEmpty(basePart))
                        basePart = part.ManufacturerPartNumber;

                    Enum.TryParse<MountingType>(part.Parameters
                        .Where(x => x.Parameter.Equals("Mounting Type", ComparisonType))
                        .Select(x => x.Value?.Replace(" ", "") ?? string.Empty)
                        .FirstOrDefault(), out var mountingTypeId);
                    var currency = digikeyResponse.Currency;
                    if (string.IsNullOrEmpty(currency))
                        currency = "USD";
                    var packageType = part.Parameters
                            ?.Where(x => x.Parameter.Equals("Supplier Device Package", ComparisonType))
                            .Select(x => x.Value)
                            .FirstOrDefault();
                    if (string.IsNullOrEmpty(packageType))
                        packageType = part.Parameters
                            ?.Where(x => x.Parameter.Equals("Package / Case", ComparisonType))
                            .Select(x => x.Value)
                            .FirstOrDefault();
                    commonParts.Add(new CommonPart
                    {
                        SupplierPartNumber = part.DigiKeyPartNumber,
                        Supplier = "DigiKey",
                        ManufacturerPartNumber = part.ManufacturerPartNumber,
                        Manufacturer = part.Manufacturer?.Value ?? string.Empty,
                        Description = part.ProductDescription + "\r\n" + part.DetailedDescription,
                        ImageUrl = part.PrimaryPhoto,
                        DatasheetUrls = new List<string> { part.PrimaryDatasheet ?? string.Empty },
                        ProductUrl = part.ProductUrl,
                        Status = part.ProductStatus,
                        Currency = currency,
                        AdditionalPartNumbers = additionalPartNumbers,
                        BasePartNumber = basePart,
                        MountingTypeId = (int)mountingTypeId,
                        PackageType = packageType,
                        Cost = lineItem.UnitPrice,
                        QuantityAvailable = lineItem.Quantity,
                        Reference = lineItem.CustomerReference,
                    });
                }
            }
            foreach (var part in commonParts)
            {
                part.PartType = DeterminePartType(part, partTypes);
                part.Keywords = DetermineKeywordsFromPart(part, partTypes);
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

        private async Task<IServiceResult<ExternalOrderResponse?>> GetExternalMouserOrderAsync(string orderId)
        {
            var user = _requestContext.GetUserContext();
            var mouserApi = await _integrationApiFactory.CreateAsync<Integrations.MouserApi>(user?.UserId ?? 0);
            if (!((MouserConfiguration)mouserApi.Configuration).IsOrdersConfigured)
                return ServiceResult<ExternalOrderResponse>.Create("Mouser Ordering Api is not enabled. Please configure your Mouser API settings and add an Ordering Api key.", nameof(Integrations.MouserApi));

            var apiResponse = await mouserApi.GetOrderAsync(orderId);
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<ExternalOrderResponse>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<ExternalOrderResponse>.Create(apiResponse.Errors, apiResponse.ApiName);
            var mouserOrderResponse = (Order?)apiResponse.Response;
            if (mouserOrderResponse != null)
            {
                var lineItems = mouserOrderResponse.OrderLines;
                var commonParts = new List<CommonPart>();
                var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
                // look up every part by digikey part number
                foreach (var lineItem in lineItems)
                {
                    // get details on this digikey part
                    if (string.IsNullOrEmpty(lineItem.MouserPartNumber))
                        continue;
                    var partResponse = await mouserApi.GetProductDetailsAsync(lineItem.MouserPartNumber);
                    if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                    {
                        if (partResponse.Response != null)
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
                                DatasheetUrls = new List<string> { part.DataSheetUrl ?? string.Empty },
                                ProductUrl = part.ProductDetailUrl,
                                Status = part.LifecycleStatus,
                                Currency = mouserOrderResponse.CurrencyCode,
                                AdditionalPartNumbers = new List<string>(),
                                BasePartNumber = part.ManufacturerPartNumber,
                                MountingTypeId = 0,
                                PackageType = "",
                                Cost = lineItem.UnitPrice,
                                QuantityAvailable = lineItem.Quantity,
                                Reference = lineItem.CartItemCustPartNumber,
                            });
                        }
                    }
                }

                foreach (var part in commonParts)
                {
                    part.PartType = DeterminePartType(part, partTypes);
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
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

            return ServiceResult<ExternalOrderResponse>.Create("Error", nameof(MouserApi));
        }

        public async Task<IServiceResult<Product?>> GetBarcodeInfoProductAsync(string barcode, ScannedBarcodeType barcodeType)
        {
            var user = _requestContext.GetUserContext();
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
            if (!digikeyApi.IsEnabled)
                return ServiceResult<Product?>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            // currently only supports DigiKey, as Mouser barcodes are part numbers
            var response = new PartResults();
            var digikeyResponse = new ProductBarcodeResponse();
            if (digikeyApi.IsEnabled)
            {
                var apiResponse = await digikeyApi.GetBarcodeDetailsAsync(barcode, barcodeType);
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
                    var partResponse = await digikeyApi.GetProductDetailsAsync(digikeyResponse.DigiKeyPartNumber);
                    if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                    {
                        var part = (Product?)partResponse.Response;
                        return ServiceResult<Product?>.Create(part);
                    }
                }
            }

            return ServiceResult<Product?>.Create(null);
        }

        public async Task<IServiceResult<PartResults?>> GetBarcodeInfoAsync(string barcode, ScannedBarcodeType barcodeType)
        {
            var user = _requestContext.GetUserContext();
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
            if (!digikeyApi.IsEnabled)
                return ServiceResult<PartResults>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            // currently only supports DigiKey, as Mouser barcodes are part numbers
            var response = new PartResults();
            var digikeyResponse = new ProductBarcodeResponse();
            if (digikeyApi.IsEnabled)
            {
                var apiResponse = await digikeyApi.GetBarcodeDetailsAsync(barcode, barcodeType);
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
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
                    var partResponse = await digikeyApi.GetProductDetailsAsync(digikeyResponse.DigiKeyPartNumber);
                    if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                    {
                        var part = (Product?)partResponse.Response;
                        if (part != null)
                        {
                            var additionalPartNumbers = new List<string>();
                            var basePart = part.Parameters
                                .Where(x => x.Parameter.Equals("Base Part Number", ComparisonType))
                                .Select(x => x.Value)
                                .FirstOrDefault();
                            if (!string.IsNullOrEmpty(basePart))
                                additionalPartNumbers.Add(basePart);
                            Enum.TryParse<MountingType>(part.Parameters
                                .Where(x => x.Parameter.Equals("Mounting Type", ComparisonType))
                                .Select(x => x.Value?.Replace(" ", ""))
                                .FirstOrDefault(), out var mountingTypeId);
                            var currency = string.Empty;
                            if (string.IsNullOrEmpty(currency))
                                currency = "USD";
                            var packageType = part.Parameters
                                ?.Where(x => x.Parameter.Equals("Supplier Device Package", ComparisonType))
                                .Select(x => x.Value)
                                .FirstOrDefault();
                            if (string.IsNullOrEmpty(packageType))
                                packageType = part.Parameters
                                    ?.Where(x => x.Parameter.Equals("Package / Case", ComparisonType))
                                    .Select(x => x.Value)
                                    .FirstOrDefault();
                            response.Parts.Add(new CommonPart
                            {
                                Supplier = "DigiKey",
                                SupplierPartNumber = part.DigiKeyPartNumber,
                                BasePartNumber = basePart ?? part.ManufacturerPartNumber,
                                AdditionalPartNumbers = additionalPartNumbers,
                                Manufacturer = part.Manufacturer?.Value ?? string.Empty,
                                ManufacturerPartNumber = part.ManufacturerPartNumber,
                                Cost = part.UnitPrice,
                                Currency = currency,
                                DatasheetUrls = new List<string> { part.PrimaryDatasheet ?? string.Empty },
                                Description = part.ProductDescription + "\r\n" + part.DetailedDescription,
                                ImageUrl = part.PrimaryPhoto,
                                PackageType = part.Parameters
                                    ?.Where(x => x.Parameter.Equals("Package / Case", ComparisonType))
                                    .Select(x => x.Value)
                                    .FirstOrDefault(),
                                MountingTypeId = (int)mountingTypeId,
                                PartType = "",
                                ProductUrl = part.ProductUrl,
                                Status = part.ProductStatus,
                                QuantityAvailable = digikeyResponse.Quantity
                            });
                        }
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
                            QuantityAvailable = digikeyResponse.Quantity
                        });
                    }
                }
            }

            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            foreach (var part in response.Parts)
            {
                part.PartType = DeterminePartType(part, partTypes);
                part.Keywords = DetermineKeywordsFromPart(part, partTypes);
            }

            return ServiceResult<PartResults>.Create(response);
        }

        public async Task<IServiceResult<PartResults?>> GetPartInformationAsync(string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "")
        {
            const int maxImagesPerSupplier = 5;
            const int maxImagesTotal = 10;
            var totalImages = 0;
            var user = _requestContext.GetUserContext();
            //var context = await _contextFactory.CreateDbContextAsync();
            var swarmApi = await _integrationApiFactory.CreateAsync<Integrations.SwarmApi>(user?.UserId ?? 0);
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
            var mouserApi = await _integrationApiFactory.CreateAsync<Integrations.MouserApi>(user?.UserId ?? 0);
            var octopartApi = await _integrationApiFactory.CreateAsync<Integrations.OctopartApi>(user?.UserId ?? 0);
            var arrowApi = await _integrationApiFactory.CreateAsync<Integrations.ArrowApi>(user?.UserId ?? 0);

            var datasheets = new List<string>();
            var response = new PartResults();
            var swarmResponse = new SwarmApi.Response.SearchPartResponse();
            var digikeyResponse = new KeywordSearchResponse();
            var mouserResponse = new SearchResultsResponse();
            var arrowResponse = new ItemServiceResult();
            var searchKeywords = partNumber;

            if (digikeyApi.Configuration.IsConfigured)
            {
                var apiResponse = await digikeyApi.SearchAsync(searchKeywords, partType, mountingType);
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
                else if (apiResponse.Errors?.Any() == true)
                    return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                digikeyResponse = (KeywordSearchResponse?)apiResponse.Response;
                if (digikeyResponse != null)
                {
                    // if no part found, and it's numerical, try getting barcode info
                    if (digikeyResponse.Products.Any() != true)
                    {
                        var isNumber = Regex.IsMatch(searchKeywords, @"^\d+$");
                        if (isNumber)
                        {
                            var barcode = searchKeywords;
                            var barcodeResult = await GetBarcodeInfoProductAsync(barcode, ScannedBarcodeType.Product);
                            digikeyResponse = new KeywordSearchResponse();
                            if (barcodeResult.Response != null)
                                digikeyResponse.Products.Add(barcodeResult.Response);
                        }
                    }

                    // if no part found, look up part if a supplier part number is provided
                    if (digikeyResponse.Products.Any() != true && !string.IsNullOrEmpty(supplierPartNumbers))
                    {
                        var supplierPartNumberParts = supplierPartNumbers.Split(',');
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
                                    var partResponse = await digikeyApi.GetProductDetailsAsync(supplierPartNumber);
                                    if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                                    {
                                        var part = (Product?)partResponse.Response;
                                        if (part != null)
                                            digikeyResponse.Products.Add(part);
                                    }
                                }
                            }
                        }
                    }

                    // if no part found, try treating the part number as a supplier part number
                    if (digikeyResponse.Products.Any() != true && !string.IsNullOrEmpty(supplierPartNumbers))
                    {
                        var supplierPartNumber = searchKeywords;
                        // try looking it up via the digikey part number
                        var partResponse = await digikeyApi.GetProductDetailsAsync(supplierPartNumber);
                        if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                        {
                            var part = (Product?)partResponse.Response;
                            if (part != null)
                                digikeyResponse.Products.Add(part);
                        }
                    }
                }
            }
            
            if (mouserApi.Configuration.IsConfigured)
            {
                var apiResponse = await mouserApi.SearchAsync(searchKeywords, partType, mountingType);
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
                else if (apiResponse.Errors?.Any() == true)
                    return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                mouserResponse = (SearchResultsResponse?)apiResponse.Response;
            }
            
            if (octopartApi.Configuration.IsConfigured)
            {
                var octopartResponse = await octopartApi.GetDatasheetsAsync(partNumber);
                datasheets.AddRange((ICollection<string>?)octopartResponse.Response ?? new List<string>());
            }
            
            if (swarmApi.Configuration.IsConfigured)
            {
                var apiResponse = await swarmApi.SearchAsync(partNumber, partType, mountingType);
                if (apiResponse.Errors?.Any() == true)
                    return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                swarmResponse = (SwarmApi.Response.SearchPartResponse?)apiResponse.Response ?? new SwarmApi.Response.SearchPartResponse();
            }

            if (arrowApi.Configuration.IsConfigured)
            {
                var apiResponse = await arrowApi.SearchAsync(searchKeywords, partType, mountingType);
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
                else if (apiResponse.Errors?.Any() == true)
                    return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                arrowResponse = (ItemServiceResult?)apiResponse.Response;
            }

            // todo: cache part types (I think we already have it somewhere)
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            var productImageUrls = new List<NameValuePair<string>>();
            var datasheetUrls = new List<NameValuePair<DatasheetSource>>();

            ProcessSwarmResponse(partNumber, response, swarmResponse, partTypes, productImageUrls, datasheetUrls);
            if (digikeyResponse != null)
                await ProcessDigikeyResponseAsync(partNumber, response, digikeyResponse, productImageUrls, datasheetUrls);
            if (mouserResponse != null)
                await ProcessMouserResponseAsync(partNumber, response, mouserResponse, productImageUrls, datasheetUrls);
            if (arrowResponse != null)
                await ProcessArrowResponseAsync(partNumber, response, arrowResponse, productImageUrls, datasheetUrls);
            SetPartTypesAndKeywords(response, partTypes);

            // apply ranking
            response.Parts = response.Parts
                // return unique results
                .DistinctBy(x => new { Supplier = x.Supplier ?? string.Empty, SupplierPartNumber = x.SupplierPartNumber ?? string.Empty })
                // order by source
                .OrderBy(x => x.Rank)
                .ThenByDescending(x => x.QuantityAvailable)
                .ThenBy(x => x.BasePartNumber)
                .ThenBy(x => x.Status)
                .ToList();

            // compute full list of images
            response.ProductImages = productImageUrls.DistinctBy(x => x.Value).ToList();
            response.Datasheets = datasheetUrls.DistinctBy(x => x.Value).ToList();

            return ServiceResult<PartResults>.Create(response);

            void ProcessSwarmResponse(string partNumber, PartResults response, SwarmApi.Response.SearchPartResponse swarmResponse, ICollection<PartType> partTypes, List<NameValuePair<string>> productImageUrls, List<NameValuePair<DatasheetSource>> datasheetUrls)
            {
                var imagesAdded = 0;
                foreach (var swarmPart in swarmResponse.Parts)
                {
                    var part = new PartNumber
                    {
                        AlternateDescription = swarmPart.AlternateDescription,
                        AlternateNames = swarmPart.AlternateNames,
                        CreatedFromSupplierId = swarmPart.CreatedFromSupplierId,
                        DateCreatedUtc = swarmPart.DateCreatedUtc,
                        DatePrunedUtc = swarmPart.DatePrunedUtc,
                        DefaultImageId = swarmPart.DefaultImageId,
                        DefaultImageResourcePath = swarmPart.DefaultImageResourcePath,
                        DefaultImageResourceSourceUrl = swarmPart.DefaultImageResourceSourceUrl,
                        Description = swarmPart.Description,
                        Name = swarmPart.Name,
                        PartNumberId = swarmPart.PartNumberId,
                        PartTypeId = swarmPart.PartTypeId,
                        PrimaryDatasheetId = swarmPart.PrimaryDatasheetId,
                        ResourceId = swarmPart.ResourceId,
                        Source = (DataSource)(int)swarmPart.Source,
                        SwarmPartNumberId = swarmPart.SwarmPartNumberId,
                        PartNumberManufacturers = swarmPart.PartNumberManufacturers?.Select(x => new PartNumberManufacturer
                        {
                            AlternateDescription = x.AlternateDescription,
                            AlternateNames = x.AlternateNames,
                            CreatedFromSupplierId = x.CreatedFromSupplierId,
                            Datasheets = x.Datasheets.Select(d => new DatasheetBasic
                            {
                                BasePartNumber = d.BasePartNumber,
                                DatasheetId = d.DatasheetId,
                                DocumentType = (PdfDocumentTypes)(int)d.DocumentType,
                                ImageCount = d.ImageCount,
                                ManufacturerName = d.ManufacturerName,
                                OriginalUrl = d.OriginalUrl,
                                PageCount = d.PageCount,
                                ProductUrl = d.ProductUrl,
                                ResourceId = d.ResourceId,
                                ResourcePath = d.ResourcePath,
                                ResourceSourceUrl = d.ResourceSourceUrl,
                                ShortDescription = d.ShortDescription,
                                Title = d.Title
                            }).ToList(),
                            DateCreatedUtc = x.DateCreatedUtc,
                            DatePrunedUtc = x.DatePrunedUtc,
                            DefaultPartNumberManufacturerImageMetadataId = x.DefaultPartNumberManufacturerImageMetadataId,
                            //Description = x.Description,
                            ImageMetadata = x.ImageMetadata.Select(i => new PartNumberManufacturerImageMetadata
                            {
                                CreatedFromSupplierId = i.CreatedFromSupplierId,
                                ImageId = i.ImageId,
                                ImageType = (ImageTypes)(int)i.ImageType,
                                IsDefault = i.IsDefault,
                                OriginalUrl = i.OriginalUrl,
                                PartNumberManufacturerId = i.PartNumberManufacturerId,
                                PartNumberManufacturerImageMetadataId = i.PartNumberManufacturerImageMetadataId,
                                ResourcePath = i.ResourcePath,
                                ResourceSourceUrl = i.ResourceSourceUrl
                            }).ToList(),
                            IsObsolete = x.IsObsolete,
                            Keywords = x.Keywords.Select(k => new Keyword
                            {
                                KeywordId = k.KeywordId,
                                Name = k.Name
                            }).ToList(),
                            ManufacturerId = x.ManufacturerId,
                            ManufacturerName = x.ManufacturerName,
                            Name = x.Name,
                            Package = x.Package.Select(p => new Package
                            {
                                Name = p.Name,
                                PackageId = p.PackageId,
                                PinCount = p.PinCount,
                                SizeDepthMm = p.SizeDepthMm,
                                SizeHeightMm = p.SizeHeightMm,
                                SizeWidthMm = p.SizeWidthMm
                            }).ToList(),
                            Parametrics = x.Parametrics.Select(p => new PartNumberManufacturerParametric
                            {
                                Name = p.Name,
                                ParametricType = (ParametricTypes)(int)p.ParametricType,
                                PartNumberManufacturerId = p.PartNumberManufacturerId,
                                PartNumberManufacturerParametricId = p.PartNumberManufacturerParametricId,
                                Units = (ParametricUnits?)(int?)p.Units,
                                ValueAsBool = p.ValueAsBool,
                                ValueAsDouble = p.ValueAsDouble,
                                ValueAsString = p.ValueAsString
                            }).ToList(),
                            PartNumberId = x.PartNumberId,
                            PartNumberManufacturerId = x.PartNumberManufacturerId,
                            PartTypeId = x.PartTypeId,
                            PrimaryDatasheetId = x.PrimaryDatasheetId,
                            Source = (DataSource)(int)x.Source,
                            Suppliers = x.Suppliers.Select(s => new PartNumberManufacturerSupplierBasic
                            {
                                Cost = s.Cost,
                                Currency = s.Currency,
                                DateCreatedUtc = s.DateCreatedUtc,
                                FactoryLeadTime = s.FactoryLeadTime,
                                FactoryStockAvailable = s.FactoryStockAvailable,
                                MinimumOrderQuantity = s.MinimumOrderQuantity,
                                Packaging = s.Packaging,
                                PartNumberManufacturerId = s.PartNumberManufacturerId,
                                PartNumberManufacturerSupplierId = s.PartNumberManufacturerSupplierId,
                                ProductUrl = s.ProductUrl,
                                QuantityAvailable = s.QuantityAvailable,
                                StockLastUpdatedUtc = s.StockLastUpdatedUtc,
                                SupplierId = s.SupplierId,
                                SupplierName = s.SupplierName,
                                SupplierPartNumber = s.SupplierPartNumber
                            }).ToList(),
                            SwarmPartNumberId = x.SwarmPartNumberId
                        }).ToList()
                    };
                    var defaultImageUrl = GetDefaultImageUrl(part);
                    if (defaultImageUrl != null)
                        productImageUrls.Add(new NameValuePair<string>(part.Name, defaultImageUrl));
                    if (part.PartNumberManufacturers?.Any() == true)
                    {
                        foreach (var manufacturerPart in part.PartNumberManufacturers)
                        {
                            foreach (var image in manufacturerPart.ImageMetadata.OrderByDescending(x => x.IsDefault)
                                         .ThenBy(x => x.ImageType))
                            {
                                var imageUrl = GetImageUrl(image);
                                if (!string.IsNullOrEmpty(imageUrl) && !productImageUrls.Any(x =>
                                                                        x.Value.Equals(imageUrl, ComparisonType))
                                                                    && imagesAdded < maxImagesPerSupplier &&
                                                                    totalImages < maxImagesTotal)
                                {
                                    imagesAdded++;
                                    totalImages++;
                                    productImageUrls.Add(new NameValuePair<string>(manufacturerPart.Name, imageUrl));
                                }
                            }

                            foreach (var datasheet in manufacturerPart.Datasheets.OrderByDescending(x =>
                                         x.DatasheetId == manufacturerPart.PrimaryDatasheetId))
                            {
                                var datasheetUrl = GetDatasheetUrl(datasheet);
                                var datasheetCoverImageUrl = GetDatasheetCoverImageUrl(datasheet);
                                if (!string.IsNullOrEmpty(datasheetUrl) && !datasheetUrls.Any(x =>
                                        x.Value.DatasheetUrl.Equals(datasheetUrl, ComparisonType)))
                                {
                                    datasheetUrls.Add(new NameValuePair<DatasheetSource>(manufacturerPart.Name,
                                        new DatasheetSource(datasheetCoverImageUrl, datasheetUrl,
                                            datasheet.Title ?? manufacturerPart.Name, datasheet?.ShortDescription ?? string.Empty,
                                            datasheet?.ManufacturerName ?? string.Empty)));
                                }
                            }

                            var mountingType = manufacturerPart.Parametrics
                                .Where(x => x.Name.Equals("Mounting Type", ComparisonType))
                                .Select(x => x.ValueAsString)
                                .FirstOrDefault();
                            var mountingTypeId = MountingType.ThroughHole;
                            Enum.TryParse<MountingType>(mountingType, out mountingTypeId);

                            foreach (var supplierPart in manufacturerPart.Suppliers)
                            {
                                response.Parts.Add(new CommonPart
                                {
                                    Rank = 0,
                                    SwarmPartNumberManufacturerId = manufacturerPart.PartNumberManufacturerId,
                                    Supplier = supplierPart.SupplierName,
                                    SupplierPartNumber = supplierPart.SupplierPartNumber,
                                    BasePartNumber = part.Name,
                                    Manufacturer = manufacturerPart.ManufacturerName,
                                    ManufacturerPartNumber = manufacturerPart.Name,
                                    Cost = supplierPart.Cost ?? 0,
                                    Currency = supplierPart.Currency,
                                    Description = !string.IsNullOrEmpty(manufacturerPart.Description)
                                        ? manufacturerPart.Description
                                        : part.Description,
                                    DatasheetUrls = GetDatasheetUrls(manufacturerPart),
                                    ImageUrl = GetDefaultManufacturerImageUrl(manufacturerPart),
                                    PackageType = manufacturerPart.Package.Select(x => x.Name).FirstOrDefault() ??
                                                  string.Empty,
                                    MountingTypeId = (int)mountingTypeId,
                                    PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name)
                                        .FirstOrDefault() ?? string.Empty,
                                    ProductUrl = supplierPart.ProductUrl,
                                    Status = manufacturerPart.IsObsolete ? "Inactive" : "Active",
                                    QuantityAvailable = supplierPart.QuantityAvailable ?? 0,
                                    MinimumOrderQuantity = supplierPart.MinimumOrderQuantity ?? 0,
                                    FactoryStockAvailable = supplierPart.FactoryStockAvailable ?? 0,
                                    FactoryLeadTime = supplierPart.FactoryLeadTime,
                                });
                            }
                        }
                    }
                }

                static ICollection<string> GetDatasheetUrls(PartNumberManufacturer part)
                {
                    var urls = new List<string>();
                    if (part.Datasheets.Any())
                    {
                        foreach (var datasheet in part.Datasheets)
                        {
                            var datasheetUrl = GetDatasheetUrl(datasheet);
                            if (!string.IsNullOrEmpty(datasheetUrl) && !urls.Contains(datasheetUrl))
                                urls.Add(datasheetUrl);
                        }
                    }
                    return urls;
                }

                static string? GetDefaultManufacturerImageUrl(PartNumberManufacturer part)
                {
                    var firstImage = part.ImageMetadata?
                        .OrderByDescending(x => x.IsDefault)
                        .ThenBy(x => x.ImageType)
                        .FirstOrDefault();
                    if (firstImage != null)
                    {
                        return GetImageUrl(firstImage);
                    }
                    return null;
                }

                static string GetImageUrl(PartNumberManufacturerImageMetadata image)
                {
                    return $"https://{image.ResourceSourceUrl}/{image.ResourcePath}_{image.ImageId}.png";
                }

                static string GetDefaultImageUrl(PartNumber part)
                {
                    if (part.DefaultImageId != null && !string.IsNullOrEmpty(part.DefaultImageResourcePath) && !string.IsNullOrEmpty(part.DefaultImageResourceSourceUrl))
                        return $"https://{part.DefaultImageResourceSourceUrl}/{part.DefaultImageResourcePath}_{part.DefaultImageId}.png";
                    return string.Empty;
                }
            }

            Task ProcessDigikeyResponseAsync(string partNumber, PartResults response, KeywordSearchResponse? digikeyResponse, List<NameValuePair<string>> productImageUrls, List<NameValuePair<DatasheetSource>> datasheetUrls)
            {
                if (digikeyResponse == null || !digikeyResponse.Products.Any()) return Task.CompletedTask;

                var imagesAdded = 0;
                var digiKeyDatasheetUrls = digikeyResponse.Products
                    .Where(x => !string.IsNullOrEmpty(x.PrimaryDatasheet))
                    .Select(x => x.PrimaryDatasheet)
                    .ToList();
                // todo: finish
                /*var existingDatasheets = await context.Datasheets
                    .Where(x => digiKeyDatasheetUrls.Contains(x.OriginalUrl))
                    .ToListAsync();
                var existingDatasheetUrls = _mapper.Map<List<DatasheetBasic>>(existingDatasheets, options => GetMappingOptions(options));*/
                foreach (var part in digikeyResponse.Products)
                {
                    var additionalPartNumbers = new List<string>();
                    var basePart = part.Parameters
                        .Where(x => x.Parameter.Equals("Base Part Number", ComparisonType))
                        .Select(x => x.Value)
                        .FirstOrDefault();
                    if (!string.IsNullOrEmpty(basePart))
                        additionalPartNumbers.Add(basePart);
                    else
                    {
                        if (part.ManufacturerPartNumber.Contains(partNumber, ComparisonType))
                            basePart = partNumber;
                    }

                    if (string.IsNullOrEmpty(basePart))
                        basePart = part.ManufacturerPartNumber;
                    var mountingTypeId = MountingType.ThroughHole;
                    Enum.TryParse<MountingType>(part.Parameters
                        .Where(x => x.Parameter.Equals("Mounting Type", ComparisonType))
                        .Select(x => x.Value?.Replace(" ", ""))
                        .FirstOrDefault(), out mountingTypeId);
                    var currency = digikeyResponse.SearchLocaleUsed.Currency;
                    if (string.IsNullOrEmpty(currency))
                        currency = "USD";
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
                        && !productImageUrls.Any(x => x.Value?.Equals(part.PrimaryPhoto, ComparisonType) == true)
                        && imagesAdded < maxImagesPerSupplier && totalImages < maxImagesTotal)
                    {
                        productImageUrls.Add(new NameValuePair<string>(part.ManufacturerPartNumber, part.PrimaryPhoto));
                        imagesAdded++;
                        totalImages++;
                    }

                    // if there is a datasheet that hasn't been added, add it
                    if (!string.IsNullOrEmpty(part.PrimaryDatasheet) && !datasheetUrls.Any(x => x.Value?.DatasheetUrl?.Equals(part.PrimaryDatasheet, ComparisonType) == true))
                    {
                        // if we have this datasheet already as a processed datasheet, use it instead to get the cover image and title
                        // todo: finish
                        //var existingDatasheet = existingDatasheetUrls.FirstOrDefault(x => x.OriginalUrl == part.PrimaryDatasheet);
                        DatasheetBasic? existingDatasheet = null;
                        if (existingDatasheet != null)
                        {
                            var datasheetUrl = GetDatasheetUrl(existingDatasheet);
                            var datasheetCoverImageUrl = GetDatasheetCoverImageUrl(existingDatasheet);
                            datasheetUrls.Add(new NameValuePair<DatasheetSource>(part.ManufacturerPartNumber, new DatasheetSource(datasheetCoverImageUrl, datasheetUrl, existingDatasheet.Title ?? part.ManufacturerPartNumber, existingDatasheet.ShortDescription ?? string.Empty, existingDatasheet.ManufacturerName ?? part.Manufacturer?.Value ?? string.Empty)));
                        }
                        else
                        {
                            // datasheetUrls.Add(new NameValuePair<DatasheetSource>(part.ManufacturerPartNumber, new DatasheetSource($"https://{_repositoryConfig.Datasheets.PublicDomainName}/{MissingDatasheetCoverName}", part.PrimaryDatasheet, part.ManufacturerPartNumber, "", part.Manufacturer.Value ?? string.Empty)));
                        }
                    }
                    response.Parts.Add(new CommonPart
                    {
                        Rank = 1,
                        SwarmPartNumberManufacturerId = null,
                        Supplier = "DigiKey",
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
                return Task.CompletedTask;
            }

            Task ProcessMouserResponseAsync(string partNumber, PartResults response, SearchResultsResponse? mouserResponse, List<NameValuePair<string>> productImageUrls, List<NameValuePair<DatasheetSource>> datasheetUrls)
            {
                if (mouserResponse == null || mouserResponse.SearchResults == null) return Task.CompletedTask;
                var imagesAdded = 0;
                var mouserDatasheetUrls = mouserResponse.SearchResults.Parts
                    .Select(x => x.DataSheetUrl)
                    .ToList();
                // todo: finish
                /*var existingDatasheets = await context.Datasheets
                    .Where(x => mouserDatasheetUrls.Contains(x.OriginalUrl))
                    .ToListAsync();
                var existingDatasheetUrls = _mapper.Map<List<DatasheetBasic>>(existingDatasheets, options => GetMappingOptions(options));*/
                foreach (var part in mouserResponse.SearchResults.Parts)
                {
                    var mountingTypeId = MountingType.ThroughHole;
                    var currency = part.PriceBreaks?.OrderBy(x => x.Quantity).FirstOrDefault()?.Currency;
                    if (string.IsNullOrEmpty(currency))
                        currency = "USD";
                    int.TryParse(part.Min, out var minimumOrderQuantity);
                    int.TryParse(part.FactoryStock, out var factoryStockAvailable);
                    if (!string.IsNullOrEmpty(part.ImagePath)
                        && !productImageUrls.Any(x => x.Value.Equals(part.ImagePath, ComparisonType))
                        && imagesAdded < maxImagesPerSupplier && totalImages < maxImagesTotal)
                    {
                        if (!string.IsNullOrEmpty(part.ManufacturerPartNumber))
                        {
                            productImageUrls.Add(new NameValuePair<string>(part.ManufacturerPartNumber,
                                part.ImagePath));
                            imagesAdded++;
                            totalImages++;
                        }
                    }

                    // if there is a datasheet that hasn't been added, add it
                    if (!string.IsNullOrEmpty(part.DataSheetUrl)
                        && !datasheetUrls.Any(x => x.Value.DatasheetUrl.Equals(part.DataSheetUrl, ComparisonType)))
                    {
                        // if we have this datasheet already as a processed datasheet, use it instead to get the cover image and title
                        // todo: finish
                        //var existingDatasheet = existingDatasheetUrls.FirstOrDefault(x => x.OriginalUrl == part.DataSheetUrl);
                        DatasheetBasic? existingDatasheet = null;
                        if (existingDatasheet != null)
                        {
                            var datasheetUrl = GetDatasheetUrl(existingDatasheet);
                            var datasheetCoverImageUrl = GetDatasheetCoverImageUrl(existingDatasheet);
                            if (!string.IsNullOrEmpty(part.ManufacturerPartNumber))
                                datasheetUrls.Add(new NameValuePair<DatasheetSource>(part.ManufacturerPartNumber, new DatasheetSource(datasheetCoverImageUrl, datasheetUrl, existingDatasheet.Title ?? part.ManufacturerPartNumber, existingDatasheet?.ShortDescription ?? string.Empty, existingDatasheet?.ManufacturerName ?? part.Manufacturer ?? string.Empty)));
                        }
                        else
                        {
                            // datasheetUrls.Add(new NameValuePair<DatasheetSource>(part.ManufacturerPartNumber, new DatasheetSource($"https://{_repositoryConfig.Datasheets.PublicDomainName}/{MissingDatasheetCoverName}", part.DataSheetUrl, part.ManufacturerPartNumber, "", part.Manufacturer)));
                        }
                    }

                    var basePart = string.Empty;
                    if (part.ManufacturerPartNumber?.Contains(partNumber, ComparisonType) == true)
                        basePart = partNumber;
                    if (string.IsNullOrEmpty(basePart))
                        basePart = partNumber;
                    response.Parts.Add(new CommonPart
                    {
                        Rank = 2,
                        SwarmPartNumberManufacturerId = null,
                        Supplier = "Mouser",
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
                return Task.CompletedTask;
            }

            Task ProcessArrowResponseAsync(string partNumber, PartResults response, ItemServiceResult? arrowResponse, List<NameValuePair<string>> productImageUrls, List<NameValuePair<DatasheetSource>> datasheetUrls)
            {
                if (arrowResponse == null || arrowResponse.Data == null || !arrowResponse.Data.Any()) return Task.CompletedTask;

                return Task.CompletedTask;
            }

            void SetPartTypesAndKeywords(PartResults response, ICollection<PartType> partTypes)
            {
                foreach (var part in response.Parts)
                {
                    part.PartType = DeterminePartType(part, partTypes);
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }
            }

            static string GetDatasheetUrl(DatasheetBasic datasheet)
            {
                return $"https://{datasheet.ResourceSourceUrl}/{datasheet.ResourcePath}.pdf";
            }

            static string GetDatasheetCoverImageUrl(DatasheetBasic datasheet)
            {
                if (datasheet.ImageCount > 0)
                    return $"https://{datasheet.ResourceSourceUrl}/{datasheet.ResourcePath}_1.png";
                return $"https://{datasheet.ResourceSourceUrl}/{MissingDatasheetCoverName}";
            }
        }

        private ICollection<string> DetermineKeywordsFromPart(CommonPart part, ICollection<PartType> partTypes)
        {
            // part type
            // important parts from description
            // alternate series numbers etc
            var keywords = new List<string>();
            var possiblePartTypes = GetMatchingPartTypes(part, partTypes);
            foreach (var possiblePartType in possiblePartTypes)
            {
                if (!string.IsNullOrEmpty(possiblePartType.Key.Name) && !keywords.Contains(possiblePartType.Key.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    keywords.Add(possiblePartType.Key.Name.ToLower());
                }
            }

            if (!string.IsNullOrEmpty(part.ManufacturerPartNumber) && !keywords.Contains(part.ManufacturerPartNumber, StringComparer.InvariantCultureIgnoreCase))
                keywords.Add(part.ManufacturerPartNumber.ToLower());
            var desc = part.Description?.ToLower().Split(new[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            // add the first 4 words of desc
            var wordCount = 0;
            var ignoredWords = new[] { "and", "the", "in", "or", "in", "a", };
            if (desc != null)
            {
                foreach (var word in desc)
                {
                    if (!ignoredWords.Contains(word, StringComparer.InvariantCultureIgnoreCase) &&
                        !keywords.Contains(word, StringComparer.InvariantCultureIgnoreCase))
                    {
                        keywords.Add(word.ToLower());
                        wordCount++;
                    }

                    if (wordCount >= 4)
                        break;
                }
            }

            foreach (var basePart in part.AdditionalPartNumbers)
                if (!keywords.Contains(basePart, StringComparer.InvariantCultureIgnoreCase))
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
                if (metadata.Description?.IndexOf(partType.Name, ComparisonType) >= 0)
                    addPart = true;
                if (metadata.DetailedDescription?.IndexOf(partType.Name, ComparisonType) >= 0)
                    addPart = true;
                if (metadata.PartNumber?.IndexOf(partType.Name, ComparisonType) >= 0)
                    addPart = true;
                if (metadata.DatasheetUrl?.IndexOf(partType.Name, ComparisonType) >= 0)
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
                if (part.Description?.IndexOf(partType.Name, ComparisonType) >= 0)
                    addPart = true;
                if (part.ManufacturerPartNumber?.IndexOf(partType.Name, ComparisonType) >= 0)
                    addPart = true;
                foreach (var datasheet in part.DatasheetUrls)
                    if (datasheet.IndexOf(partType.Name, ComparisonType) >= 0)
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

        public async Task<string> DeterminePartTypeAsync(CommonPart part)
        {
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            var possiblePartTypes = GetMatchingPartTypes(part, partTypes);
            return possiblePartTypes
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key?.Name)
                .FirstOrDefault() ?? string.Empty;
        }

        public string DeterminePartType(CommonPart part, ICollection<PartType> partTypes)
        {
            var possiblePartTypes = GetMatchingPartTypes(part, partTypes);
            return possiblePartTypes
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key?.Name)
                .FirstOrDefault() ?? string.Empty;
        }
    }
}
