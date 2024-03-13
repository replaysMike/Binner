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
using Binner.Model.Integrations.Nexar;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Binner.Model.Swarm;
using Binner.StorageProvider.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;
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

        public async Task<IServiceResult<PartResults?>> GetPartInformationAsync(string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "")
        {
            const int maxImagesPerSupplier = 5;
            const int maxImagesTotal = 10;
            var response = new PartResults();
            var totalImages = 0;
            var user = _requestContext.GetUserContext();
            //var context = await _contextFactory.CreateDbContextAsync();
            var swarmApi = await _integrationApiFactory.CreateAsync<Integrations.SwarmApi>(user?.UserId ?? 0);
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
            var mouserApi = await _integrationApiFactory.CreateAsync<Integrations.MouserApi>(user?.UserId ?? 0);
            var nexarApi = await _integrationApiFactory.CreateAsync<Integrations.NexarApi>(user?.UserId ?? 0);
            var arrowApi = await _integrationApiFactory.CreateAsync<Integrations.ArrowApi>(user?.UserId ?? 0);
            if (partNumber.StartsWith("[)>"))
            {
                if (digikeyApi.Configuration.IsConfigured)
                {
                    // 2d barcode scan requires decode first
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

                // continue with lookup
            }
            if (string.IsNullOrEmpty(partNumber))
            {
                // return empty result, invalid request
                return ServiceResult<PartResults>.Create(response);
            }

            var apiResponses = new Dictionary<string, Model.Integrations.ApiResponseState>();
            var datasheets = new List<string>();
            var swarmResponse = new SwarmApi.Response.SearchPartResponse();
            var digikeyResponse = new KeywordSearchResponse();
            var mouserResponse = new SearchResultsResponse();
            var arrowResponse = new ArrowResponse();
            var nexarResponse = new NexarPartResults();
            var searchKeywords = partNumber;

            if (digikeyApi.Configuration.IsConfigured)
            {
                IApiResponse? apiResponse = null;
                try
                {
                    apiResponse = await digikeyApi.SearchAsync(searchKeywords, partType, mountingType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{nameof(DigikeyApi)}]: {ex.GetBaseException().Message}");
                    apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(DigikeyApi));
                }
                apiResponses.Add(nameof(DigikeyApi), new Model.Integrations.ApiResponseState(false, apiResponse));

                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
                if (apiResponse.Warnings?.Any() == true)
                {
                    _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
                }
                if (apiResponse.Errors?.Any() == true)
                {
                    _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
                    // return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                }

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
                            IServiceResult<Product?> barcodeResult = null;
                            try
                            {
                                barcodeResult = await GetBarcodeInfoProductAsync(barcode, ScannedBarcodeType.Product);
                                digikeyResponse = new KeywordSearchResponse();
                                if (barcodeResult.Response != null)
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
                                    IApiResponse? partResponse = null;
                                    try
                                    {
                                        partResponse = await digikeyApi.GetProductDetailsAsync(supplierPartNumber);
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
                    if (digikeyResponse.Products.Any() != true && !string.IsNullOrEmpty(supplierPartNumbers))
                    {
                        var supplierPartNumber = searchKeywords;
                        // try looking it up via the digikey part number
                        try
                        {
                            var partResponse = await digikeyApi.GetProductDetailsAsync(supplierPartNumber);
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
                            apiResponse.Errors.Add($"Error fetching product details on supplier part number '{WebUtility.HtmlEncode(supplierPartNumbers)}': {ex.GetBaseException().Message}");

                        }
                    }

                    apiResponses[nameof(DigikeyApi)].IsSuccess = digikeyResponse.Products.Any();
                }
            }

            if (mouserApi.Configuration.IsConfigured)
            {
                IApiResponse? apiResponse = null;
                try
                {
                    apiResponse = await mouserApi.SearchAsync(searchKeywords, partType, mountingType);
                }
                catch (MouserErrorsException ex)
                {
                    _logger.LogError(ex, $"[{nameof(MouserApi)}]: {string.Join(", ", ex.Errors)}");
                    apiResponse = new ApiResponse(ex.Errors.Select(x => x.Message).ToList(), nameof(MouserApi));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{nameof(MouserApi)}]: {ex.GetBaseException().Message}");
                    apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(MouserApi));
                }
                apiResponses.Add(nameof(MouserApi), new Model.Integrations.ApiResponseState(false, apiResponse));
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults?>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
                if (apiResponse.Warnings?.Any() == true)
                {
                    _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
                }
                if (apiResponse.Errors?.Any() == true)
                {
                    _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
                    //return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                }

                mouserResponse = (SearchResultsResponse?)apiResponse.Response;
                apiResponses[nameof(MouserApi)].IsSuccess = mouserResponse?.SearchResults?.Parts?.Any() == true;
            }

            if (nexarApi.Configuration.IsConfigured)
            {
                IApiResponse? apiResponse = null;
                try
                {
                    apiResponse = await nexarApi.SearchAsync(searchKeywords, partType, mountingType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{nameof(NexarApi)}]: {ex.GetBaseException().Message}");
                    apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(NexarApi));
                }
                apiResponses.Add(nameof(NexarApi), new Model.Integrations.ApiResponseState(false, apiResponse));
                if (apiResponse.RequiresAuthentication)
                    return ServiceResult<PartResults>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
                if (apiResponse.Warnings?.Any() == true)
                {
                    _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
                }
                if (apiResponse.Errors?.Any() == true)
                {
                    _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
                    //return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                }
                nexarResponse = (NexarPartResults?)apiResponse.Response;
                apiResponses[nameof(NexarApi)].IsSuccess = nexarResponse?.Parts?.Any() == true;
            }

            if (swarmApi.Configuration.IsConfigured)
            {
                IApiResponse? apiResponse = null;
                //apiResponse = await swarmApi.SearchAsync(partNumber, partType, mountingType);
                try
                {
                    apiResponse = await swarmApi.SearchAsync(partNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{nameof(SwarmApi)}]: {ex.GetBaseException().Message}");
                    apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(SwarmApi));

                }
                apiResponses.Add(nameof(SwarmApi), new Model.Integrations.ApiResponseState(false, apiResponse));

                if (apiResponse.Warnings?.Any() == true)
                {
                    _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
                }
                if (apiResponse.Errors?.Any() == true)
                {
                    _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
                    //return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                }

                swarmResponse = (SwarmApi.Response.SearchPartResponse?)apiResponse.Response ?? new SwarmApi.Response.SearchPartResponse();
                apiResponses[nameof(SwarmApi)].IsSuccess = swarmResponse.Parts?.Any() == true;
            }

            if (arrowApi.Configuration.IsConfigured)
            {
                IApiResponse? apiResponse = null;
                try
                {
                    apiResponse = await arrowApi.SearchAsync(searchKeywords, partType, mountingType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{nameof(ArrowApi)}]: {ex.GetBaseException().Message}");
                    apiResponse = new ApiResponse(new List<string> { ex.GetBaseException().Message }, nameof(ArrowApi));
                }
                apiResponses.Add(nameof(ArrowApi), new Model.Integrations.ApiResponseState(false, apiResponse));
                if (apiResponse.Warnings?.Any() == true)
                {
                    _logger.LogWarning($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Warnings)}");
                }
                if (apiResponse.Errors?.Any() == true)
                {
                    _logger.LogError($"[{apiResponse.ApiName}]: {string.Join(". ", apiResponse.Errors)}");
                    //return ServiceResult<PartResults>.Create(apiResponse.Errors, apiResponse.ApiName);
                }

                arrowResponse = (ArrowResponse?)apiResponse.Response;
                apiResponses[nameof(ArrowApi)].IsSuccess = arrowResponse?.ItemServiceResult?.Data?.Any() == true;
            }

            if (!apiResponses.Any(x => x.Value.IsSuccess))
            {
                if (apiResponses.Any(x => x.Value.Response?.Errors.Any() == true))
                {
                    // there are errors, and no successful responses
                    var errors = apiResponses
                        .Where(x => x.Value.Response != null && x.Value.Response.Errors.Any())
                        .SelectMany(x => x.Value.Response!.Errors.Select(errorMessage => $"[{x.Value.Response.ApiName}] {errorMessage}")).ToList();
                    var apiNames = apiResponses.Where(x => x.Value.Response?.Errors.Any() == true).GroupBy(x => x.Key);
                    var apiName = "Multiple";
                    if (apiNames.Count() == 1) apiName = apiNames.First().Key;
                    return ServiceResult<PartResults>.Create(errors, apiName);
                }
            }

            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            var productImageUrls = new List<NameValuePair<string>>();
            var datasheetUrls = new List<NameValuePair<DatasheetSource>>();

            // fetch part if it's in inventory
            var inventoryPart = await GetPartAsync(new GetPartRequest { PartNumber = partNumber });
            if (inventoryPart != null && !string.IsNullOrEmpty(inventoryPart.DatasheetUrl))
                datasheetUrls.Add(new NameValuePair<DatasheetSource>(inventoryPart.ManufacturerPartNumber ?? string.Empty, new DatasheetSource($"https://{_configuration.ResourceSource}/{MissingDatasheetCoverName}", inventoryPart.DatasheetUrl, inventoryPart.ManufacturerPartNumber ?? string.Empty, inventoryPart.Description ?? string.Empty, inventoryPart.Manufacturer ?? string.Empty)));
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

            ProcessSwarmResponse(partNumber, response, swarmResponse, partTypes, productImageUrls, datasheetUrls);
            if (digikeyResponse != null)
                await ProcessDigikeyResponseAsync(partNumber, response, digikeyResponse, productImageUrls, datasheetUrls);
            if (mouserResponse != null)
                await ProcessMouserResponseAsync(partNumber, response, mouserResponse, productImageUrls, datasheetUrls);
            if (arrowResponse != null)
                await ProcessArrowResponseAsync(partNumber, response, arrowResponse, productImageUrls, datasheetUrls);
            if (nexarResponse != null)
                await ProcessNexarResponseAsync(partNumber, response, nexarResponse, productImageUrls, datasheetUrls);
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

            var serviceResult = ServiceResult<PartResults>.Create(response);
            if (apiResponses.Any(x => x.Value.Response != null && x.Value.Response.Errors.Any()))
                serviceResult.Errors = apiResponses
                    .Where(x => x.Value.Response != null && x.Value.Response.Errors.Any())
                    .SelectMany(x => x.Value.Response!.Errors.Select(errorMessage => $"[{x.Value.Response.ApiName}] {errorMessage}"));
            return serviceResult;

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
                            Package = x.Package.Select(p => new Model.Swarm.Package
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
                                if (!string.IsNullOrEmpty(imageUrl)
                                    && !productImageUrls.Any(x => x.Value?.Equals(imageUrl, ComparisonType) == true)
                                    && imagesAdded < maxImagesPerSupplier && totalImages < maxImagesTotal)
                                {
                                    imagesAdded++;
                                    totalImages++;
                                    productImageUrls.Add(new NameValuePair<string>(manufacturerPart.Name, imageUrl));
                                }
                            }

                            foreach (var datasheet in manufacturerPart.Datasheets.OrderByDescending(x => x.DatasheetId == manufacturerPart.PrimaryDatasheetId))
                            {
                                var datasheetUrl = GetDatasheetUrl(datasheet);
                                var datasheetCoverImageUrl = GetDatasheetCoverImageUrl(datasheet);
                                if (!string.IsNullOrEmpty(datasheetUrl) && !datasheetUrls.Any(x => x.Value?.DatasheetUrl.Equals(datasheetUrl, ComparisonType) == true))
                                {
                                    var datasheetSource = new DatasheetSource(datasheet.ResourceId,
                                        datasheet.ImageCount,
                                        datasheet.PageCount,
                                        datasheetCoverImageUrl,
                                        datasheetUrl,
                                        datasheet.Title ?? manufacturerPart.Name,
                                        datasheet?.ShortDescription,
                                        datasheet?.ManufacturerName,
                                        datasheet?.OriginalUrl,
                                        datasheet?.ProductUrl);
                                    datasheetUrls.Add(new NameValuePair<DatasheetSource>(manufacturerPart.Name, datasheetSource));
                                }
                            }

                            var mountingType = manufacturerPart.Parametrics
                                .Where(x => x.Name.Equals("Mounting Type", ComparisonType))
                                .Select(x => x.ValueAsString)
                                .FirstOrDefault();
                            var mountingTypeId = MountingType.None;
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
                foreach (var part in digikeyResponse.Products)
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
                            if (part.ManufacturerPartNumber.Contains(partNumber, ComparisonType))
                                basePart = partNumber;
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
                        var currency = digikeyResponse.SearchLocaleUsed.Currency;
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
                            var datasheetSource = new DatasheetSource($"https://{_configuration.ResourceSource}/{MissingDatasheetCoverName}", part.PrimaryDatasheet,
                                part.ManufacturerPartNumber, "", part.Manufacturer?.Value ?? string.Empty);
                            datasheetUrls.Add(new NameValuePair<DatasheetSource>(part.ManufacturerPartNumber, datasheetSource));
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
                }
                return Task.CompletedTask;
            }

            Task ProcessMouserResponseAsync(string partNumber, PartResults response, SearchResultsResponse? mouserResponse, List<NameValuePair<string>> productImageUrls, List<NameValuePair<DatasheetSource>> datasheetUrls)
            {
                if (mouserResponse == null || mouserResponse.SearchResults == null) return Task.CompletedTask;
                var imagesAdded = 0;
                if (mouserResponse.SearchResults.Parts != null)
                {
                    foreach (var part in mouserResponse.SearchResults.Parts)
                    {
                        var mountingTypeId = MountingType.None;
                        var basePart = string.Empty;
                        if (part.ManufacturerPartNumber?.Contains(partNumber, ComparisonType) == true)
                            basePart = partNumber;
                        if (string.IsNullOrEmpty(basePart))
                            basePart = partNumber;

                        var currency = part.PriceBreaks?.OrderBy(x => x.Quantity).FirstOrDefault()?.Currency;
                        if (string.IsNullOrEmpty(currency))
                            currency = _configuration.Locale.Currency.ToString().ToUpper();
                        int.TryParse(part.Min, out var minimumOrderQuantity);
                        int.TryParse(part.FactoryStock, out var factoryStockAvailable);
                        if (!string.IsNullOrEmpty(part.ImagePath)
                            && !productImageUrls.Any(x => x.Value?.Equals(part.ImagePath, ComparisonType) == true)
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
                            && !datasheetUrls.Any(x => x.Value?.DatasheetUrl.Equals(part.DataSheetUrl, ComparisonType) == true))
                        {
                            var datasheetSource = new DatasheetSource($"https://{_configuration.ResourceSource}/{MissingDatasheetCoverName}", part.DataSheetUrl,
                                part.ManufacturerPartNumber ?? basePart, "", part.Manufacturer ?? string.Empty);
                            datasheetUrls.Add(new NameValuePair<DatasheetSource>(part.ManufacturerPartNumber ?? partNumber, datasheetSource));
                        }

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
                }

                return Task.CompletedTask;
            }

            Task ProcessArrowResponseAsync(string partNumber, PartResults response, ArrowResponse? arrowResponse, List<NameValuePair<string>> productImageUrls, List<NameValuePair<DatasheetSource>> datasheetUrls)
            {
                if (arrowResponse == null || arrowResponse.ItemServiceResult == null || arrowResponse.ItemServiceResult.Data == null || !arrowResponse.ItemServiceResult.Data.Any()) return Task.CompletedTask;

                var imagesAdded = 0;
                if (arrowResponse.ItemServiceResult.Data.Any())
                {
                    var data = arrowResponse.ItemServiceResult.Data.FirstOrDefault();
                    if (data == null || data.PartList == null || !data.PartList.Any())
                        return Task.CompletedTask;
                    foreach (var part in data.PartList)
                    {
                        var additionalPartNumbers = new List<string>();
                        var basePart = partNumber;
                        var itemId = part.ItemId;
                        var productStatus = part.Status;
                        var manufacturerPartNumber = part.PartNum ?? partNumber;
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
                            if (imagesAdded < maxImagesPerSupplier && totalImages < maxImagesTotal && !string.IsNullOrEmpty(image.Uri))
                            {
                                productImageUrls.Add(new NameValuePair<string>(manufacturerPartNumber, image.Uri));
                                imagesAdded++;
                                totalImages++;
                            }
                        }

                        if (imagesAdded == 0)
                        {
                            images = part.Resources.Where(x => x.Type == "image_small");
                            foreach (var image in images)
                            {
                                if (imagesAdded < maxImagesPerSupplier && totalImages < maxImagesTotal && !string.IsNullOrEmpty(image.Uri))
                                {
                                    productImageUrls.Add(new NameValuePair<string>(manufacturerPartNumber, image.Uri));
                                    imagesAdded++;
                                    totalImages++;
                                }
                            }
                        }

                        var arrowDatasheets = part.Resources
                            .Where(x => x.Type == "datasheet" && !string.IsNullOrEmpty(x.Uri))
                            .Select(x => x.Uri ?? string.Empty)
                            .ToList();
                        foreach (var datasheetUri in arrowDatasheets)
                        {
                            var datasheetSource = new DatasheetSource($"https://{_configuration.ResourceSource}/{MissingDatasheetCoverName}", datasheetUri,
                                manufacturerPartNumber, "", part.Manufacturer?.MfrName ?? string.Empty);
                            datasheetUrls.Add(new NameValuePair<DatasheetSource>(manufacturerPartNumber, datasheetSource));
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

                        response.Parts.Add(new CommonPart
                        {
                            Rank = 3,
                            SwarmPartNumberManufacturerId = null,
                            Supplier = "Arrow",
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

            Task ProcessNexarResponseAsync(string partNumber, PartResults response, NexarPartResults? nexarResponse, List<NameValuePair<string>> productImageUrls, List<NameValuePair<DatasheetSource>> datasheetUrls)
            {
                if (nexarResponse == null || nexarResponse.Parts == null || !nexarResponse.Parts.Any()) return Task.CompletedTask;

                var imagesAdded = 0;
                if (nexarResponse.Parts.Any())
                {
                    foreach (var part in nexarResponse.Parts)
                    {
                        var additionalPartNumbers = new List<string>();
                        var basePart = partNumber;
                        var productStatus = ""; // todo:
                        var manufacturerPartNumber = part.ManufacturerPartNumber ?? partNumber;
                        var mountingTypeId = MountingType.None;
                        /*Enum.TryParse<MountingType>(part.
                            .Where(x => x.Parameter.Equals("Mounting Type", ComparisonType))
                            .Select(x => x.Value?.Replace(" ", ""))
                            .FirstOrDefault(), out mountingTypeId);*/
                        var packageType = part.Specs.Where(x => x.Attribute?.ShortName == "case_package")
                            .Select(x => x.Value).FirstOrDefault();
                        var productUrl = "";

                        if (!string.IsNullOrEmpty(part.BestImageUrl))
                            productImageUrls.Add(new NameValuePair<string>(manufacturerPartNumber, part.BestImageUrl));
                        foreach (var image in part.Images)
                        {
                            if (imagesAdded < maxImagesPerSupplier && totalImages < maxImagesTotal && !string.IsNullOrEmpty(image.Url))
                            {
                                productImageUrls.Add(new NameValuePair<string>(manufacturerPartNumber, image.Url));
                                imagesAdded++;
                                totalImages++;
                            }
                        }

                        if (part.BestDatasheet != null && !string.IsNullOrEmpty(part.BestDatasheet.Url))
                            datasheetUrls.Add(new NameValuePair<DatasheetSource>(manufacturerPartNumber, new DatasheetSource($"https://{_configuration.ResourceSource}/{MissingDatasheetCoverName}", part.BestDatasheet.Url, manufacturerPartNumber, "", part.Manufacturer?.Name ?? string.Empty)));
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
                                    $"https://{_configuration.ResourceSource}/{MissingDatasheetCoverName}",
                                    datasheetUri.Url,
                                    manufacturerPartNumber,
                                    "",
                                    part.Manufacturer?.Name,
                                    datasheetUri.SourceUrl,
                                    null);
                                datasheetUrls.Add(new NameValuePair<DatasheetSource>(manufacturerPartNumber, datasheetSource));
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
                                response.Parts.Add(new CommonPart
                                {
                                    Rank = 3,
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
                            response.Parts.Add(new CommonPart
                            {
                                Rank = 3,
                                SwarmPartNumberManufacturerId = null,
                                Supplier = "Octopart",
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
