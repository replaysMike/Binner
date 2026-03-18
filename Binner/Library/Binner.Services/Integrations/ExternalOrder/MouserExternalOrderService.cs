using Binner.Common.Extensions;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.Model.Integrations.Mouser;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Microsoft.Extensions.Logging;

namespace Binner.Services.Integrations.ExternalOrder
{
    /// <summary>
    /// Mouser external order service
    /// </summary>
    /// <remarks>Maps an external order response to a CommonPart</remarks>
    public class MouserExternalOrderService : ApiExternalOrderServiceBase, IMouserExternalOrderService
    {
        protected readonly WebHostServiceConfiguration _configuration;
        protected readonly IIntegrationApiFactory _integrationApiFactory;
        protected readonly IUserConfigurationService _userConfigurationService;

        public MouserExternalOrderService(WebHostServiceConfiguration configuration, IStorageProvider storageProvider, IIntegrationApiFactory integrationApiFactory, IRequestContextAccessor requestContextAccessor, IUserConfigurationService userConfigurationService, ILogger<BaseIntegrationBehavior> baseIntegrationLogger)
            : base(baseIntegrationLogger, storageProvider, requestContextAccessor)
        {
            _configuration = configuration;
            _integrationApiFactory = integrationApiFactory;
            _userConfigurationService = userConfigurationService;
        }

        public virtual async Task<IServiceResult<ExternalOrderListResponse?>> ListExternalOrdersAsync(OrderListRequest request)
        {
            var user = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            var integrationConfiguration = _userConfigurationService.GetCachedOrganizationIntegrationConfiguration(user.OrganizationId);
            var mouserApi = await _integrationApiFactory.CreateAsync<Integrations.MouserApi>(user.UserId, integrationConfiguration);
            if (!((MouserConfiguration)mouserApi.Configuration).IsOrdersConfigured)
                return ServiceResult<ExternalOrderListResponse?>.Create("Mouser Ordering Api is not enabled. Please configure your Mouser API settings and add an Ordering Api key.", nameof(Integrations.MouserApi));

            var apiResponse = await mouserApi.ListOrdersAsync(request.StartDate, request.EndDate, request.PageNumber, request.PageSize);
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<ExternalOrderListResponse?>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<ExternalOrderListResponse?>.Create(apiResponse.Errors, apiResponse.ApiName);

            var response = (OrderHistoryResponseRoot?)apiResponse.Response;
            if (response != null)
            {
                return ServiceResult<ExternalOrderListResponse?>.Create(new ExternalOrderListResponse
                {
                    Orders = response.OrderHistoryItems.Select(o => new ExternalOrderBasic
                    {
                        OrderId = o.WebOrderNumber,
                        OrderDate = o.DateCreated,
                        OrderItemsTotal = 0, // not available with mouser
                        OrderStatus = o.OrderStatusDisplay
                    }).ToList()
                });
            }
            return ServiceResult<ExternalOrderListResponse>.Create("Error", nameof(MouserApi));
        }

        public virtual async Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(OrderImportRequest request)
        {
            var user = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            var integrationConfiguration = _userConfigurationService.GetCachedOrganizationIntegrationConfiguration(user.OrganizationId);
            var mouserApi = await _integrationApiFactory.CreateAsync<Integrations.MouserApi>(user.UserId, integrationConfiguration);
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
                var mouserApiMaxOrderLineItems = ApiConstants.OrderFullPartInfoMaxRecords;
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

                    if (!request.RequestProductInfo || lineItemCount > mouserApiMaxOrderLineItems)
                    {
                        commonParts.Add(await MouserOrderLineToCommonPartAsync(lineItem, mouserOrderResponse.CurrencyCode ?? string.Empty));
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
                            commonParts.Add(await MouserPartToCommonPartAsync(part, lineItem, mouserOrderResponse.CurrencyCode ?? string.Empty));
                        }
                        else
                        {
                            messages.Add(Model.Responses.Message.FromInfo($"No additional product details available on part '{lineItem.ProductInfo.MouserPartNumber}'."));
                            // use the more minimal information provided by the order import call
                            commonParts.Add(await MouserOrderLineToCommonPartAsync(lineItem, mouserOrderResponse.CurrencyCode ?? string.Empty));
                        }
                    }
                    else
                    {
                        messages.Add(Model.Responses.Message.FromInfo($"Search API not configured, no additional product details available on part '{lineItem.ProductInfo.MouserPartNumber}'."));
                        // use the more minimal information provided by the order import call
                        commonParts.Add(await MouserOrderLineToCommonPartAsync(lineItem, mouserOrderResponse.CurrencyCode ?? string.Empty));
                    }
                }

                foreach (var part in commonParts)
                {
                    /*var partType = await DeterminePartTypeAsync(part);
                    part.PartType = partType?.Name ?? string.Empty;
                    part.PartTypeId = partType?.PartTypeId ?? 0;
                    part.ParentPartTypeId = partType?.ParentPartTypeId;*/
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }
                commonParts = await MapCommonPartIdsAsync(commonParts);
                return ServiceResult<ExternalOrderResponse?>.Create(new ExternalOrderResponse
                {
                    OrderDate = mouserOrderResponse.OrderDate,
                    OrderStatus = mouserOrderResponse.OrderStatusName ?? mouserOrderResponse.OrderStatus.ToString(),
                    OrderId = mouserOrderResponse.WebOrderId,
                    Currency = mouserOrderResponse.CurrencyCode,
                    CustomerId = mouserOrderResponse.BuyerName,
                    Amount = mouserOrderResponse.SummaryDetail?.OrderTotal.FromCurrency() ?? 0d,
                    TrackingNumber = mouserOrderResponse.DeliveryDetail?.TrackingDetails?.FirstOrDefault()?.Number ?? mouserOrderResponse.DeliveryDetail?.ShippingMethodName,
                    TrackingNumberUrl = mouserOrderResponse.DeliveryDetail?.TrackingDetails?.FirstOrDefault()?.Link,
                    Messages = messages,
                    Parts = commonParts
                });
            }

            return ServiceResult<ExternalOrderResponse>.Create("Error", nameof(MouserApi));
        }

        private async Task<PartType?> DeterminePartTypeAsync(MouserPart part, OrderHistoryLine orderLine)
        {
            // note: partTypes call is cached
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            var possiblePartTypes = GetMatchingPartTypes(part, orderLine, partTypes);
            var bestGuessPartType = possiblePartTypes
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .FirstOrDefault();
            // if we chose a parent category when there is a more specific child category available, choose it instead
            if (bestGuessPartType != null && possiblePartTypes.Any(x => x.Key.ParentPartTypeId == bestGuessPartType.PartTypeId))
                bestGuessPartType = possiblePartTypes.Where(x => x.Key.ParentPartTypeId == bestGuessPartType.PartTypeId).Select(x => x.Key).FirstOrDefault();

            // default to Other if we can't find a match
            return bestGuessPartType ?? partTypes.FirstOrDefault(x => x.Name == "Other");
        }

        private async Task<PartType?> DeterminePartTypeAsync(OrderHistoryLine orderLine)
        {
            // note: partTypes call is cached
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            var possiblePartTypes = GetMatchingPartTypes(orderLine, partTypes);

            // if IC is matched but we have more specific info, filter it out
            if (possiblePartTypes.Any())
            {
                var highestValue = possiblePartTypes.OrderByDescending(x => x.Value).FirstOrDefault();
                if ((highestValue.Key.Name == "IC" || highestValue.Key.Name == "Hardware") && possiblePartTypes.Count > 1)
                {
                    possiblePartTypes.Remove(highestValue.Key);
                }
            }

            var bestGuessPartType = possiblePartTypes
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key.Name) // if we have multiple with the same priority, order by name to ensure we always pick the same one
                .Select(x => x.Key)
                .FirstOrDefault();
            // if we chose a parent category when there is a more specific child category available, choose it instead
            if (bestGuessPartType != null && possiblePartTypes.Any(x => x.Key.ParentPartTypeId == bestGuessPartType.PartTypeId))
                bestGuessPartType = possiblePartTypes.Where(x => x.Key.ParentPartTypeId == bestGuessPartType.PartTypeId).Select(x => x.Key).FirstOrDefault();

            // default to Other if we can't find a match
            return bestGuessPartType ?? partTypes.FirstOrDefault(x => x.Name == "Other");
        }

        private async Task<CommonPart> MouserPartToCommonPartAsync(MouserPart part, OrderHistoryLine orderLine, string currencyCode)
        {
            var partType = await DeterminePartTypeAsync(part, orderLine);
            var cp = new CommonPart
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
                Cost = (decimal)orderLine.UnitPrice,
                QuantityAvailable = orderLine.Quantity,
                Quantity = orderLine.Quantity,
                Reference = orderLine.ProductInfo.CustomerPartNumber,
                Categories = new List<CommonCategory>(),
                PartType = partType?.Name ?? string.Empty,
                PartTypeId = partType?.PartTypeId ?? 0,
                ParentPartTypeId = partType?.ParentPartTypeId
            };
            if (!string.IsNullOrEmpty(part.Category)) cp.Categories.Add(new CommonCategory { Name = part.Category });
            return cp;
        }

        private async Task<CommonPart> MouserOrderLineToCommonPartAsync(OrderHistoryLine? orderLine, string currencyCode)
        {
            var partType = await DeterminePartTypeAsync(orderLine);
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
                Cost = (decimal)orderLine.UnitPrice,
                TotalCost = (decimal)orderLine.UnitPrice * orderLine.Quantity,
                QuantityAvailable = orderLine.Quantity,
                Quantity = orderLine.Quantity,
                Reference = orderLine.ProductInfo.CustomerPartNumber,
                Categories = new List<CommonCategory>(),
                PartType = partType?.Name ?? string.Empty,
                PartTypeId = partType?.PartTypeId ?? 0,
                ParentPartTypeId = partType?.ParentPartTypeId
            };
        }

        private Dictionary<PartType, int> GetMatchingPartTypes(MouserPart part, OrderHistoryLine orderLine, ICollection<PartType> partTypes)
        {
            // load all part types
            var possiblePartTypes = new Dictionary<PartType, int>();
            var description = (part.Category + " " + orderLine.ProductInfo.PartDescription).Trim();
            var descriptionWords = description?.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray() ?? Array.Empty<string>();
            for (var i = 0; i < descriptionWords.Length; i++)
                descriptionWords[i] = RemovePlurals(descriptionWords[i]);

            foreach (var partType in partTypes)
            {
                var defaultPriority = 1;
                if (string.IsNullOrEmpty(partType.Name))
                    continue;

                var partTypeName = RemovePlurals(partType.Name);
                partTypeName = partTypeName.ToLower();

                var addPart = false;
                var index = Array.IndexOf(descriptionWords, partTypeName);
                if (index >= 0)
                {
                    addPart = true;
                    // calculate a priority based on how early in the description the part type is found
                    var oldRange = descriptionWords.Length - 0;
                    var newRange = (5 - 1);
                    defaultPriority = 5 - (((index - 0) * newRange) / oldRange);

                    if (possiblePartTypes.ContainsKey(partType))
                        possiblePartTypes[partType] += defaultPriority;
                    else
                        possiblePartTypes.Add(partType, defaultPriority);

                    continue;
                }

                // check the keywords on the part type
                var keywords = partType.Keywords?.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
                for (var i = 0; i < keywords.Count; i++)
                {
                    keywords[i] = RemovePlurals(keywords[i]).ToLower();
                }
                var defaultPartType = (SystemDefaults.DefaultPartTypes?)partType.PartTypeId;
                if (defaultPartType != null)
                {
                    var info = GetPartTypeInfo(defaultPartType);
                    if (info != null && !string.IsNullOrEmpty(info.Keywords))
                        keywords.AddRange(info.Keywords.Split([','], StringSplitOptions.RemoveEmptyEntries));
                }
                keywords = keywords.Distinct().ToList();

                foreach (var keyword in keywords)
                {
                    var keywordWordCount = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    if (orderLine.ProductInfo.PartDescription?.Contains(keyword, ComparisonType) == true)
                    {
                        addPart = true;
                        defaultPriority += keywordWordCount * 1;
                    }
                    if (orderLine.ProductInfo.CustomerPartNumber?.Contains(keyword, ComparisonType) == true)
                    {
                        addPart = true;
                        defaultPriority += keywordWordCount * 1;
                    }
                }

                if (addPart)
                {
                    if (possiblePartTypes.ContainsKey(partType))
                        possiblePartTypes[partType] += defaultPriority;
                    else
                        possiblePartTypes.Add(partType, defaultPriority);
                }

            }
            return possiblePartTypes;
        }

        private Dictionary<PartType, int> GetMatchingPartTypes(OrderHistoryLine orderLine, ICollection<PartType> partTypes)
        {
            // load all part types
            var possiblePartTypes = new Dictionary<PartType, int>();
            var descriptionWords = orderLine.ProductInfo.PartDescription?.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray() ?? Array.Empty<string>();
            for (var i = 0; i < descriptionWords.Length; i++)
                descriptionWords[i] = RemovePlurals(descriptionWords[i]);

            foreach (var partType in partTypes)
            {
                var defaultPriority = 1;
                if (string.IsNullOrEmpty(partType.Name))
                    continue;

                var partTypeName = RemovePlurals(partType.Name);
                partTypeName = partTypeName.ToLower();

                var addPart = false;
                var index = Array.IndexOf(descriptionWords, partTypeName);
                if (index >= 0)
                {
                    addPart = true;
                    // calculate a priority based on how early in the description the part type is found
                    var oldRange = descriptionWords.Length - 0;
                    var newRange = (5 - 1);
                    defaultPriority = 5 - (((index - 0) * newRange) / oldRange);

                    if (possiblePartTypes.ContainsKey(partType))
                        possiblePartTypes[partType] += defaultPriority;
                    else
                        possiblePartTypes.Add(partType, defaultPriority);

                    continue;
                }

                // check the keywords on the part type
                var keywords = partType.Keywords?.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
                for (var i = 0; i < keywords.Count; i++)
                {
                    keywords[i] = RemovePlurals(keywords[i]).ToLower();
                }
                var defaultPartType = (SystemDefaults.DefaultPartTypes?)partType.PartTypeId;
                if (defaultPartType != null)
                {
                    var info = GetPartTypeInfo(defaultPartType);
                    if (info != null && !string.IsNullOrEmpty(info.Keywords))
                        keywords.AddRange(info.Keywords.Split([','], StringSplitOptions.RemoveEmptyEntries));
                }
                keywords = keywords.Distinct().ToList();

                foreach (var keyword in keywords)
                {
                    var keywordWordCount = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    if (orderLine.ProductInfo.PartDescription?.Contains(keyword, ComparisonType) == true)
                    {
                        addPart = true;
                        defaultPriority += keywordWordCount * 1;
                    }
                    if (orderLine.ProductInfo.CustomerPartNumber?.Contains(keyword, ComparisonType) == true)
                    {
                        addPart = true;
                        defaultPriority += keywordWordCount * 1;
                    }
                }

                if (addPart)
                {
                    if (possiblePartTypes.ContainsKey(partType))
                        possiblePartTypes[partType] += defaultPriority;
                    else
                        possiblePartTypes.Add(partType, defaultPriority);
                }

            }
            return possiblePartTypes;
        }
    }
}
