using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.Arrow;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Microsoft.Extensions.Logging;

namespace Binner.Services.Integrations.ExternalOrder
{
    /// <summary>
    /// Arrow external order service
    /// </summary>
    /// <remarks>Maps an external order response to a CommonPart</remarks>
    public class ArrowExternalOrderService : ApiExternalOrderServiceBase, IArrowExternalOrderService
    {
        protected readonly WebHostServiceConfiguration _configuration;
        protected readonly IIntegrationApiFactory _integrationApiFactory;
        protected readonly IUserConfigurationService _userConfigurationService;

        public ArrowExternalOrderService(WebHostServiceConfiguration configuration, IStorageProvider storageProvider, IIntegrationApiFactory integrationApiFactory, IRequestContextAccessor requestContextAccessor, IUserConfigurationService userConfigurationService, ILogger<BaseIntegrationBehavior> baseIntegrationLogger)
            : base(baseIntegrationLogger, storageProvider, requestContextAccessor)
        {
            _configuration = configuration;
            _integrationApiFactory = integrationApiFactory;
            _userConfigurationService = userConfigurationService;
        }

        public virtual async Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(OrderImportRequest request)
        {
            var user = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            var integrationConfiguration = _userConfigurationService.GetCachedOrganizationIntegrationConfiguration(user.OrganizationId);
            var arrowApi = await _integrationApiFactory.CreateAsync<Integrations.ArrowApi>(user.UserId, integrationConfiguration);
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
                        TotalCost = lineItem.UnitPrice * lineItem.Quantity,
                        QuantityAvailable = (long)lineItem.Quantity,
                        Quantity = (long)lineItem.Quantity,
                        Reference = lineItem.CustomerPartNo,
                    });
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
    }
}
