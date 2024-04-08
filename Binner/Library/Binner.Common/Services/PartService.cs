using AutoMapper;
using Binner.Common.Integrations;
using Binner.Common.Integrations.Models;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.Arrow;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Integrations.Mouser;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Binner.StorageProvider.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Part = Binner.Model.Part;

namespace Binner.Common.Services
{
    public class PartService : IPartService
    {
        private const string MissingDatasheetCoverName = "datasheetcover.png";
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;

        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly IStorageProvider _storageProvider;
        private readonly IMapper _mapper;
        private readonly IIntegrationApiFactory _integrationApiFactory;
        private readonly RequestContextAccessor _requestContext;
        private readonly ISwarmService _swarmService;
        private readonly ILogger<PartService> _logger;
        private readonly IPartTypesCache _partTypesCache;
        private readonly ILicensedService _licensedService;

        public PartService(IDbContextFactory<BinnerContext> contextFactory, WebHostServiceConfiguration configuration, ILogger<PartService> logger, IStorageProvider storageProvider, IMapper mapper, IIntegrationApiFactory integrationApiFactory, ISwarmService swarmService, RequestContextAccessor requestContextAccessor, IPartTypesCache partTypesCache, ILicensedService licensedService)
        {
            _contextFactory = contextFactory;
            _configuration = configuration;
            _logger = logger;
            _storageProvider = storageProvider;
            _mapper = mapper;
            _integrationApiFactory = integrationApiFactory;
            _requestContext = requestContextAccessor;
            _swarmService = swarmService;
            _partTypesCache = partTypesCache;
            _licensedService = licensedService;
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

        public async Task<Part?> GetPartAsync(GetPartRequest request)
        {
            if (request.PartId > 0)
                return await _storageProvider.GetPartAsync(request.PartId, _requestContext.GetUserContext());
            else
                return await _storageProvider.GetPartAsync(request.PartNumber ?? string.Empty, _requestContext.GetUserContext());
        }

        public async Task<(Part? Part, ICollection<StoredFile> StoredFiles)> GetPartWithStoredFilesAsync(GetPartRequest request)
        {
            var userContext = _requestContext.GetUserContext();
            var partEntity = await GetPartAsync(request);
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
            var user = _requestContext.GetUserContext();
            // if this part is assigned in any BOMs, remove it
            var projectPartAssignments = await _storageProvider.GetPartAssignmentsAsync(part.PartId, user);
            foreach (var partAssignment in projectPartAssignments)
            {
                await _storageProvider.RemoveProjectPartAssignmentAsync(partAssignment, user);
            }

            var success = await _storageProvider.DeletePartAsync(part, user);
            return success;
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

        public Task<ICollection<PartTypeResponse>> GetPartTypesWithPartCountsAsync()
        {
            var userContext = _requestContext.GetUserContext();
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var models = _partTypesCache.Cache
                .Where(x => x.OrganizationId == userContext.OrganizationId)
                .OrderBy(x => x.ParentPartType != null ? x.ParentPartType.Name : "")
                .ThenBy(x => x.Name)
                .ToList();
            return Task.FromResult(_mapper.Map<ICollection<PartTypeResponse>>(models));
        }

        public async Task<ICollection<PartSupplier>> GetPartSuppliersAsync(long partId)
        {
            return await _storageProvider.GetPartSuppliersAsync(partId, _requestContext.GetUserContext());
        }

        public async Task<PartSupplier> AddPartSupplierAsync(PartSupplier partSupplier)
        {
            return await _storageProvider.AddPartSupplierAsync(partSupplier, _requestContext.GetUserContext());
        }

        public async Task<PartSupplier> UpdatePartSupplierAsync(PartSupplier partSupplier)
        {
            return await _storageProvider.UpdatePartSupplierAsync(partSupplier, _requestContext.GetUserContext());
        }

        public async Task<bool> DeletePartSupplierAsync(PartSupplier partSupplier)
        {
            return await _storageProvider.DeletePartSupplierAsync(partSupplier, _requestContext.GetUserContext());
        }

        public async Task<IServiceResult<CategoriesResponse?>> GetCategoriesAsync()
        {
            var user = _requestContext.GetUserContext();
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
            if (!digikeyApi.IsEnabled)
                return ServiceResult<CategoriesResponse?>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            var apiResponse = await digikeyApi.GetCategoriesAsync();
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<CategoriesResponse?>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<CategoriesResponse?>.Create(apiResponse.Errors, apiResponse.ApiName);

            if (apiResponse.Response != null)
            {
                var digikeyResponse = (CategoriesResponse)apiResponse.Response;
                return ServiceResult<CategoriesResponse?>.Create(digikeyResponse);
            }

            return ServiceResult<CategoriesResponse?>.Create("Invalid response received", apiResponse.ApiName);
        }

        /// <summary>
        /// Get an external order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(OrderImportRequest request)
        {
            if (string.IsNullOrEmpty(request.OrderId)) throw new InvalidOperationException($"OrderId must be provided");

            switch (request.Supplier?.ToLower())
            {
                case "digikey":
                    return await GetExternalDigiKeyOrderAsync(request);
                case "mouser":
                    return await GetExternalMouserOrderAsync(request);
                case "arrow":
                    return await GetExternalArrowOrderAsync(request);
                case "tme":
                    throw new NotSupportedException($"TME order imports are not yet supported as they don't have an API for it.");
                default:
                    throw new InvalidOperationException($"Unknown supplier {request.Supplier}");
            }
        }

        private async Task<IServiceResult<ExternalOrderResponse?>> GetExternalDigiKeyOrderAsync(OrderImportRequest request)
        {
            var user = _requestContext.GetUserContext();
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
            if (!digikeyApi.IsEnabled)
                return ServiceResult<ExternalOrderResponse?>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            var apiResponse = await digikeyApi.GetOrderAsync(request.OrderId);
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<ExternalOrderResponse?>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<ExternalOrderResponse?>.Create(apiResponse.Errors, apiResponse.ApiName);

            var messages = new List<Model.Responses.Message>();
            var digikeyResponse = (OrderSearchResponse?)apiResponse.Response ?? new OrderSearchResponse();

            var lineItems = digikeyResponse.LineItems;
            var commonParts = new List<CommonPart>();
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());

            var digikeyApiMaxOrderLineItems = 50;
            var isLargeOrder = lineItems.Count > digikeyApiMaxOrderLineItems;

            if (isLargeOrder)
            {
                // only supply the information provided by the order once we hit this limit
                messages.Add(Model.Responses.Message.FromInfo($"This order is too large to get metadata on every product. Only the first {digikeyApiMaxOrderLineItems} products will have full metadata information available (DigiKey Api Limitation)."));
            }

            var lineItemCount = 0;
            var errorsEncountered = 0;
            // look up every part by digikey part number
            foreach (var lineItem in lineItems)
            {
                lineItemCount++;

                // get details on this digikey part
                if (string.IsNullOrEmpty(lineItem.DigiKeyPartNumber))
                    continue;

                if (!request.RequestProductInfo || lineItemCount > digikeyApiMaxOrderLineItems || errorsEncountered > 0)
                {
                    commonParts.Add(DigiKeyLineItemToCommonPart(lineItem));
                    continue;
                }

                IApiResponse? partResponse = null;
                var productResponseSuccess = false;
                try
                {
                    partResponse = await digikeyApi.GetProductDetailsAsync(lineItem.DigiKeyPartNumber);
                    if (!partResponse.RequiresAuthentication && partResponse.Errors.Any() == false)
                        productResponseSuccess = true;
                    if (partResponse.RequiresAuthentication)
                    {
                        messages.Add(Model.Responses.Message.FromError($"Failed to get product details on part '{lineItem.DigiKeyPartNumber}'. Api: Requires authentication."));
                        errorsEncountered++;
                    }
                    if (partResponse.Errors.Any() == true)
                    {
                        messages.Add(Model.Responses.Message.FromError($"Failed to get product details on part '{lineItem.DigiKeyPartNumber}'. Api Errors: {string.Join(",", partResponse.Errors)}"));
                        errorsEncountered++;
                    }
                }
                catch (Exception ex)
                {
                    // likely we have been throttled
                    messages.Add(Model.Responses.Message.FromError($"Failed to get product details on part '{lineItem.DigiKeyPartNumber}'. Exception: {ex.GetBaseException().Message}"));
                    errorsEncountered++;
                }

                if (productResponseSuccess && partResponse?.Response != null)
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

                    commonParts.Add(DigiKeyPartToCommonPart(part, currency, additionalPartNumbers, basePart, (int)mountingTypeId, packageType, lineItem));
                }
                else
                {
                    messages.Add(Model.Responses.Message.FromInfo($"No additional product details available on part '{lineItem.DigiKeyPartNumber}'."));
                    // use the more minimal information provided by the order import call
                    commonParts.Add(DigiKeyLineItemToCommonPart(lineItem));
                }
            }
            foreach (var part in commonParts)
            {
                part.PartType = DeterminePartType(part, partTypes);
                part.Keywords = DetermineKeywordsFromPart(part, partTypes);
            }
            return ServiceResult<ExternalOrderResponse?>.Create(new ExternalOrderResponse
            {
                OrderDate = digikeyResponse.ShippingDetails.Any() ? DateTime.Parse(digikeyResponse.ShippingDetails.First().DateTransaction ?? DateTime.MinValue.ToString()) : DateTime.MinValue,
                Currency = digikeyResponse.Currency,
                CustomerId = digikeyResponse.CustomerId.ToString(),
                Amount = lineItems.Sum(x => x.TotalPrice),
                TrackingNumber = digikeyResponse.ShippingDetails.Any() ? digikeyResponse.ShippingDetails.First().TrackingUrl : "",
                Messages = messages,
                Parts = commonParts
            });
        }

        private CommonPart DigiKeyPartToCommonPart(Product part, string currency, ICollection<string> additionalPartNumbers, string? basePart, int mountingTypeId, string? packageType, LineItem lineItem) => new CommonPart
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
            MountingTypeId = mountingTypeId,
            PackageType = packageType,
            Cost = lineItem.UnitPrice,
            QuantityAvailable = lineItem.Quantity,
            Reference = lineItem.CustomerReference,
        };

        private CommonPart DigiKeyLineItemToCommonPart(LineItem lineItem) => new CommonPart
        {
            SupplierPartNumber = lineItem.DigiKeyPartNumber,
            Supplier = "DigiKey",
            ManufacturerPartNumber = string.Empty,
            Manufacturer = string.Empty,
            Description = lineItem.ProductDescription,
            Cost = lineItem.UnitPrice,
            QuantityAvailable = lineItem.Quantity,
            Reference = lineItem.CustomerReference,
        };

        private async Task<IServiceResult<ExternalOrderResponse?>> GetExternalMouserOrderAsync(OrderImportRequest request)
        {
            var user = _requestContext.GetUserContext();
            var mouserApi = await _integrationApiFactory.CreateAsync<Integrations.MouserApi>(user?.UserId ?? 0);
            if (!((MouserConfiguration)mouserApi.Configuration).IsOrdersConfigured)
                return ServiceResult<ExternalOrderResponse?>.Create("Mouser Ordering Api is not enabled. Please configure your Mouser API settings and add an Ordering Api key.", nameof(Integrations.MouserApi));

            var messages = new List<Model.Responses.Message>();
            var apiResponse = await mouserApi.GetOrderAsync(request.OrderId);
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<ExternalOrderResponse?>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<ExternalOrderResponse?>.Create(apiResponse.Errors, apiResponse.ApiName);
            var mouserOrderResponse = (OrderHistory?)apiResponse.Response;
            if (mouserOrderResponse != null)
            {
                var lineItems = mouserOrderResponse.OrderLines;
                var commonParts = new List<CommonPart>();
                var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
                var mouserApiMaxOrderLineItems = 25;
                var isLargeOrder = lineItems.Count > mouserApiMaxOrderLineItems;

                if (isLargeOrder)
                {
                    // only supply the information provided by the order once we hit this limit
                    messages.Add(Model.Responses.Message.FromInfo($"This order is too large to get metadata on every product. Only the first {mouserApiMaxOrderLineItems} products will have full metadata information available (Mouser Api Limitation)."));
                }

                // look up every part by mouser part number
                var lineItemCount = 0;
                var errorsEncountered = 0;
                foreach (var lineItem in lineItems)
                {
                    lineItemCount++;

                    // get details on this mouser part
                    if (string.IsNullOrEmpty(lineItem.ProductInfo.MouserPartNumber))
                        continue;

                    if (!request.RequestProductInfo || lineItemCount > mouserApiMaxOrderLineItems || errorsEncountered > 0)
                    {
                        commonParts.Add(MouserOrderLineToCommonPart(lineItem, mouserOrderResponse.CurrencyCode ?? string.Empty));
                        continue;
                    }

                    // if search api is configured, request additional information for each part
                    if (((MouserConfiguration)mouserApi.Configuration).IsConfigured)
                    {
                        // request additional information for the part as orders doesn't return much
                        IApiResponse? partResponse = null;
                        var productResponseSuccess = false;
                        try
                        {

                            partResponse = await mouserApi.GetProductDetailsAsync(lineItem.ProductInfo.MouserPartNumber);
                            if (!partResponse.RequiresAuthentication && partResponse.Errors.Any() == false)
                                productResponseSuccess = true;
                            if (partResponse.RequiresAuthentication)
                            {
                                messages.Add(Model.Responses.Message.FromError($"Failed to get product details on part '{lineItem.ProductInfo.MouserPartNumber}'. Api: Requires authentication."));
                                errorsEncountered++;
                            }
                            if (partResponse.Errors.Any() == true)
                            {
                                messages.Add(Model.Responses.Message.FromError($"Failed to get product details on part '{lineItem.ProductInfo.MouserPartNumber}'. Api Errors: {string.Join(",", partResponse.Errors)}"));
                                errorsEncountered++;
                            }
                        }
                        catch (Exception ex)
                        {
                            // likely we have been throttled
                            messages.Add(Model.Responses.Message.FromError($"Failed to get product details on part '{lineItem.ProductInfo.MouserPartNumber}'. Exception: {ex.GetBaseException().Message}"));
                            errorsEncountered++;
                        }
                        if (productResponseSuccess && partResponse?.Response != null)
                        {
                            var searchResults = (ICollection<MouserPart>)partResponse.Response;
                            // convert the part to a common part
                            var part = searchResults.First();
                            commonParts.Add(MouserPartToCommonPart(part, lineItem, mouserOrderResponse.CurrencyCode ?? string.Empty));
                        }
                        else
                        {
                            messages.Add(Model.Responses.Message.FromInfo($"No additional product details available on part '{lineItem.ProductInfo.MouserPartNumber}'."));
                            // use the more minimal information provided by the order import call
                            commonParts.Add(MouserOrderLineToCommonPart(lineItem, mouserOrderResponse.CurrencyCode ?? string.Empty));
                        }
                    }
                    else
                    {
                        messages.Add(Model.Responses.Message.FromInfo($"Search API not configured, no additional product details available on part '{lineItem.ProductInfo.MouserPartNumber}'."));
                        // use the more minimal information provided by the order import call
                        commonParts.Add(MouserOrderLineToCommonPart(lineItem, mouserOrderResponse.CurrencyCode ?? string.Empty));
                    }
                }

                foreach (var part in commonParts)
                {
                    part.PartType = DeterminePartType(part, partTypes);
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }

                return ServiceResult<ExternalOrderResponse?>.Create(new ExternalOrderResponse
                {
                    OrderDate = mouserOrderResponse.OrderDate,
                    Currency = mouserOrderResponse.CurrencyCode,
                    CustomerId = mouserOrderResponse.BuyerName,
                    Amount = double.Parse(mouserOrderResponse.SummaryDetail?.OrderTotal.Replace("$", "") ?? "0"),
                    TrackingNumber = mouserOrderResponse.DeliveryDetail?.ShippingMethodName,
                    Messages = messages,
                    Parts = commonParts
                });
            }

            return ServiceResult<ExternalOrderResponse>.Create("Error", nameof(MouserApi));
        }

        private CommonPart MouserPartToCommonPart(MouserPart part, OrderHistoryLine orderLine, string currencyCode)
        {
            return new CommonPart
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
                Currency = currencyCode,
                AdditionalPartNumbers = new List<string>(),
                BasePartNumber = part.ManufacturerPartNumber,
                MountingTypeId = 0,
                PackageType = "",
                Cost = orderLine.UnitPrice,
                QuantityAvailable = orderLine.Quantity,
                Reference = orderLine.ProductInfo.CustomerPartNumber,
            };
        }

        private CommonPart MouserOrderLineToCommonPart(OrderHistoryLine? orderLine, string currencyCode)
        {
            return new CommonPart
            {
                SupplierPartNumber = orderLine.ProductInfo.MouserPartNumber,
                Supplier = "Mouser",
                ManufacturerPartNumber = orderLine.ProductInfo.ManufacturerPartNumber,
                Manufacturer = orderLine.ProductInfo.ManufacturerName,
                Description = orderLine.ProductInfo.PartDescription,
                //ImageUrl = part.ImagePath,
                //DatasheetUrls = new List<string> { part.DataSheetUrl ?? string.Empty },
                //ProductUrl = lineItem.ProductDetailUrl,
                //Status = part.LifecycleStatus,
                Currency = currencyCode,
                AdditionalPartNumbers = new List<string>(),
                BasePartNumber = orderLine.ProductInfo.ManufacturerPartNumber,
                MountingTypeId = 0,
                PackageType = "",
                Cost = orderLine.UnitPrice,
                QuantityAvailable = orderLine.Quantity,
                Reference = orderLine.ProductInfo.CustomerPartNumber,
            };
        }

        private async Task<IServiceResult<ExternalOrderResponse?>> GetExternalArrowOrderAsync(OrderImportRequest request)
        {
            var user = _requestContext.GetUserContext();
            var arrowApi = await _integrationApiFactory.CreateAsync<Integrations.ArrowApi>(user?.UserId ?? 0);
            if (!((ArrowConfiguration)arrowApi.Configuration).IsConfigured)
                return ServiceResult<ExternalOrderResponse?>.Create("Arrow Ordering Api is not enabled. Please configure your Arrow API settings and ensure a Username and Api key is set.", nameof(Integrations.ArrowApi));
            if (string.IsNullOrEmpty(request.Password))
                return ServiceResult<ExternalOrderResponse?>.Create("Arrow orders require your account password! (it's a requirement of their API)", nameof(Integrations.ArrowApi));

            var apiResponse = await arrowApi.GetOrderAsync(request.OrderId, new Dictionary<string, string> { { "username", request.Username ?? string.Empty }, { "password", request.Password ?? string.Empty } });
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<ExternalOrderResponse?>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<ExternalOrderResponse?>.Create(apiResponse.Errors, apiResponse.ApiName);
            var arrowOrderResponse = (OrderResponse?)apiResponse.Response;
            if (arrowOrderResponse != null)
            {
                var lineItems = arrowOrderResponse.WebItems;
                var commonParts = new List<CommonPart>();
                var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
                // look up every part by part number
                foreach (var lineItem in lineItems)
                {
                    // get details on this part
                    if (string.IsNullOrEmpty(lineItem.ItemNo))
                        continue;

                    commonParts.Add(new CommonPart
                    {
                        Supplier = "Arrow",
                        SupplierPartNumber = lineItem.ItemNo,
                        ManufacturerPartNumber = lineItem.ItemNo,
                        //Manufacturer = part.Manufacturer,
                        Description = lineItem.Description,
                        //ImageUrl = part.ImagePath,
                        //DatasheetUrls = new List<string> { part.DataSheetUrl ?? string.Empty },
                        //ProductUrl = part.ProductDetailUrl,
                        //Status = part.LifecycleStatus,
                        Currency = arrowOrderResponse.CurrencyCode,
                        AdditionalPartNumbers = new List<string>(),
                        BasePartNumber = lineItem.ItemNo,
                        MountingTypeId = 0,
                        PackageType = "",
                        Cost = lineItem.UnitPrice,
                        QuantityAvailable = (long)lineItem.Quantity,
                        Reference = lineItem.CustomerPartNo,
                    });
                }

                foreach (var part in commonParts)
                {
                    part.PartType = DeterminePartType(part, partTypes);
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }

                return ServiceResult<ExternalOrderResponse?>.Create(new ExternalOrderResponse
                {
                    OrderDate = DateTime.MinValue,
                    Currency = arrowOrderResponse.CurrencyCode,
                    CustomerId = "",
                    Amount = arrowOrderResponse.TotalAmount ?? 0d,
                    TrackingNumber = "",
                    Parts = commonParts
                });
            }

            return ServiceResult<ExternalOrderResponse?>.Create("Error", nameof(ArrowApi));
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
                    var is2dBarcode = barcode.StartsWith("[)>");
                    if (!is2dBarcode)
                    {
                        digikeyResponse = new ProductBarcodeResponse
                        {
                            DigiKeyPartNumber = barcode
                        };
                    }
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
                        return ServiceResult<PartResults>.NotFound();
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

        private bool IsBarcodeScan(string partNumber) => !string.IsNullOrEmpty(partNumber) && partNumber.StartsWith("[)>");

        public async Task<IServiceResult<PartResults?>> GetPartInformationAsync(string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "")
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

            // continue with lookup
            if (string.IsNullOrEmpty(partNumber))
            {
                // return empty result, invalid request
                return ServiceResult<PartResults>.Create("No part number requested!", "Multiple");
            }

            // fetch all part types
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());

            // fetch part to see if it's in inventory
            var inventoryPart = await GetPartAsync(new GetPartRequest { PartNumber = partNumber });
            if (inventoryPart != null)
            {
                // add in manually specified part suppliers if available
                var partSuppliers = await GetPartSuppliersAsync(inventoryPart.PartId);
                if (partSuppliers?.Any() == true)
                {
                    foreach (var supplier in partSuppliers)
                    {
                        response.Parts.Add(new CommonPart
                        {
                            Rank = 0,
                            Supplier = supplier.Name,
                            SupplierPartNumber = supplier.SupplierPartNumber,
                            ManufacturerPartNumber = inventoryPart.ManufacturerPartNumber,
                            BasePartNumber = inventoryPart.PartNumber,
                            Cost = supplier.Cost ?? 0,
                            Currency = _configuration.Locale.Currency.ToString().ToUpper(),
                            ImageUrl = supplier.ImageUrl,
                            ProductUrl = supplier.ProductUrl,
                            QuantityAvailable = supplier.QuantityAvailable,
                            MinimumOrderQuantity = supplier.MinimumOrderQuantity,
                            // unique to these types of responses
                            PartSupplierId = supplier.PartSupplierId
                        });
                    }
                }
            }

            // fetch part information from enabled API's
            var partInformationProvider = new PartInformationProvider(_integrationApiFactory, _logger, _configuration);
            PartInformationResults? partInfoResults;
            try
            {
                partInfoResults = await partInformationProvider.FetchPartInformationAsync(partNumber, partType, mountingType, supplierPartNumbers, user?.UserId ?? 0, partTypes, inventoryPart);
                if (partInfoResults.PartResults.Parts.Any())
                    response.Parts.AddRange(partInfoResults.PartResults.Parts);
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

            // Combine all the product images and datasheets into the root response and remove duplicates
            response.ProductImages = partInfoResults.PartResults.ProductImages.DistinctBy(x => x.Value).ToList();
            response.Datasheets = partInfoResults.PartResults.Datasheets.DistinctBy(x => x.Value).ToList();

            // iterate through the responses and inject PartType objects and keywords
            InjectPartTypesAndKeywords(response, partTypes);

            var serviceResult = ServiceResult<PartResults>.Create(response);
            if (partInfoResults.ApiResponses.Any(x => x.Value.Response != null && x.Value.Response.Errors.Any()))
                serviceResult.Errors = partInfoResults.ApiResponses
                    .Where(x => x.Value.Response != null && x.Value.Response.Errors.Any())
                    .SelectMany(x => x.Value.Response!.Errors.Select(errorMessage => $"[{x.Value.Response.ApiName}] {errorMessage}"));
            return serviceResult;

            void InjectPartTypesAndKeywords(PartResults response, ICollection<PartType> partTypes)
            {
                foreach (var part in response.Parts)
                {
                    part.PartType = DeterminePartType(part, partTypes);
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }
            }

            async Task<string> DecodeBarcode(string partNumber, IUserContext? user)
            {
                var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
                if (digikeyApi.Configuration.IsConfigured)
                {
                    // 2d barcode scan requires decode first to get the partNumber being searched
                    var barcodeInfo = await GetBarcodeInfoAsync(partNumber, ScannedBarcodeType.Product);
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
