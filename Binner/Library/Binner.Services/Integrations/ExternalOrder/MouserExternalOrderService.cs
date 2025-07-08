using Binner.Common.Extensions;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.Model.Integrations.Mouser;
using Binner.Model.Requests;
using Binner.Model.Responses;

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

        public MouserExternalOrderService(WebHostServiceConfiguration configuration, IStorageProvider storageProvider, IIntegrationApiFactory integrationApiFactory, IRequestContextAccessor requestContextAccessor, IUserConfigurationService userConfigurationService)
            : base(storageProvider, requestContextAccessor)
        {
            _configuration = configuration;
            _integrationApiFactory = integrationApiFactory;
            _userConfigurationService = userConfigurationService;
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
                    var partType = await DeterminePartTypeAsync(part);
                    part.PartType = partType?.Name ?? string.Empty;
                    part.PartTypeId = partType?.PartTypeId ?? 0;
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }
                commonParts = await MapCommonPartIdsAsync(commonParts);
                return ServiceResult<ExternalOrderResponse?>.Create(new ExternalOrderResponse
                {
                    OrderDate = mouserOrderResponse.OrderDate,
                    Currency = mouserOrderResponse.CurrencyCode,
                    CustomerId = mouserOrderResponse.BuyerName,
                    Amount = mouserOrderResponse.SummaryDetail?.OrderTotal.FromCurrency() ?? 0d,
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
                Quantity = orderLine.Quantity,
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
                TotalCost = orderLine.UnitPrice * orderLine.Quantity,
                QuantityAvailable = orderLine.Quantity,
                Quantity = orderLine.Quantity,
                Reference = orderLine.ProductInfo.CustomerPartNumber,
            };
        }
    }
}
