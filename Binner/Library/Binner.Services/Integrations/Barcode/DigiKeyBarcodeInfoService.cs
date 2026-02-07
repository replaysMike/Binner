using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations.DigiKey;
using Microsoft.Extensions.Logging;
using System.Data;
using V3 = Binner.Model.Integrations.DigiKey.V3;
using V4 = Binner.Model.Integrations.DigiKey.V4;

namespace Binner.Services.Integrations.Barcode
{
    public class DigiKeyBarcodeInfoService : ApiBarcodeInfoServiceBase, IDigiKeyBarcodeInfoService
    {
        protected readonly WebHostServiceConfiguration _configuration;
        protected readonly IIntegrationApiFactory _integrationApiFactory;
        protected readonly IUserConfigurationService _userConfigurationService;

        public DigiKeyBarcodeInfoService(WebHostServiceConfiguration configuration, IStorageProvider storageProvider, IIntegrationApiFactory integrationApiFactory, IRequestContextAccessor requestContextAccessor, IUserConfigurationService userConfigurationService, ILogger<BaseIntegrationBehavior> baseIntegrationLogger)
            : base(baseIntegrationLogger, storageProvider, requestContextAccessor)
        {
            _configuration = configuration;
            _integrationApiFactory = integrationApiFactory;
            _userConfigurationService = userConfigurationService;
        }

        public virtual async Task<IServiceResult<PartResults?>> GetBarcodeInfoAsync(string barcode, ScannedLabelType barcodeType)
        {
            var user = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            var integrationConfiguration = _userConfigurationService.GetCachedOrganizationIntegrationConfiguration(user.UserId);
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user.UserId, integrationConfiguration);
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
                        if (partResponse.Response is V3.Product)
                        {
                            var part = (V3.Product?)partResponse.Response;
                            if (part != null)
                                response.Parts.Add(await DigikeyV3ProductToCommonPartAsync(part, digikeyResponse));
                        }
                        if (partResponse.Response is V4.ProductDetails)
                        {
                            var part = (V4.ProductDetails?)partResponse.Response;
                            if (part != null)
                                response.Parts.Add(DigikeyV4ProductDetailsToCommonPart(part, digikeyResponse));
                        }
                    }
                    else
                    {
                        return ServiceResult<PartResults>.NotFound();
                    }
                }
            }

            if (response.Parts.Any())
            {
                var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
                foreach (var part in response.Parts)
                {
                    var partType = await DeterminePartTypeAsync(part);
                    part.PartType = partType?.Name ?? string.Empty;
                    part.PartTypeId = partType?.PartTypeId ?? 0;
                    part.ParentPartTypeId = partType?.ParentPartTypeId;
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }
                response.Parts = await MapCommonPartIdsAsync(response.Parts);
            }

            return ServiceResult<PartResults>.Create(response);
        }

        protected virtual async Task<CommonPart> DigikeyV3ProductToCommonPartAsync(V3.Product part, ProductBarcodeResponse barcodeResponse)
        {
            var localeConfiguration = _userConfigurationService.GetCachedUserConfiguration();

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
                currency = localeConfiguration.Currency.ToString().ToUpper();
            var packageType = part.Parameters
                ?.Where(x => x.Parameter.Equals("Supplier Device Package", ComparisonType))
                .Select(x => x.Value)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(packageType))
                packageType = part.Parameters
                    ?.Where(x => x.Parameter.Equals("Package / Case", ComparisonType))
                    .Select(x => x.Value)
                    .FirstOrDefault();
            var result = new CommonPart
            {
                Supplier = "DigiKey",
                SupplierPartNumber = part.DigiKeyPartNumber,
                BasePartNumber = basePart ?? part.ManufacturerPartNumber,
                AdditionalPartNumbers = additionalPartNumbers,
                Manufacturer = part.Manufacturer?.Value ?? string.Empty,
                ManufacturerPartNumber = part.ManufacturerPartNumber,
                TotalCost = (decimal)part.UnitPrice,
                Cost = (decimal)part.UnitPrice,
                Currency = currency,
                DatasheetUrls = new List<string> { part.PrimaryDatasheet ?? string.Empty },
                Description = part.ProductDescription + "\r\n" + part.DetailedDescription,
                ImageUrl = part.PrimaryPhoto,
                PackageType = part.Parameters
                    ?.Where(x => x.Parameter.Equals("Package / Case", ComparisonType))
                    .Select(x => x.Value)
                    .FirstOrDefault(),
                MountingTypeId = (int)mountingTypeId,
                ProductUrl = part.ProductUrl,
                Status = part.ProductStatus,
                QuantityAvailable = barcodeResponse.Quantity,
                Quantity = barcodeResponse.Quantity,
            };
            var partType = await DeterminePartTypeAsync(result);
            result.PartType = partType?.Name ?? string.Empty;
            result.PartTypeId = partType?.PartTypeId ?? 0;
            result.ParentPartTypeId = partType?.ParentPartTypeId;
            return result;
        }

        private CommonPart DigikeyV4ProductDetailsToCommonPart(V4.ProductDetails details, ProductBarcodeResponse barcodeResponse)
        {
            var localeConfiguration = _userConfigurationService.GetCachedUserConfiguration();

            var part = details.Product;
            var additionalPartNumbers = new List<string>();
            var basePart = part.Parameters
                .Where(x => x.ParameterText.Equals("Base Part Number", ComparisonType))
                .Select(x => x.ValueText)
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(basePart))
                additionalPartNumbers.Add(basePart);
            Enum.TryParse<MountingType>(part.Parameters
                .Where(x => x.ParameterText.Equals("Mounting Type", ComparisonType))
                .Select(x => x.ValueText?.Replace(" ", ""))
                .FirstOrDefault(), out var mountingTypeId);
            var currency = string.Empty;
            if (string.IsNullOrEmpty(currency))
                currency = localeConfiguration.Currency.ToString().ToUpper();
            var packageType = part.Parameters
                ?.Where(x => x.ParameterText.Equals("Supplier Device Package", ComparisonType))
                .Select(x => x.ValueText)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(packageType))
                packageType = part.Parameters
                    ?.Where(x => x.ParameterText.Equals("Package / Case", ComparisonType))
                    .Select(x => x.ValueText)
                    .FirstOrDefault();
            return new CommonPart
            {
                Supplier = "DigiKey",
                SupplierPartNumber = part.ProductVariations.FirstOrDefault()?.DigiKeyProductNumber ?? string.Empty,
                BasePartNumber = basePart ?? part.ManufacturerProductNumber,
                AdditionalPartNumbers = additionalPartNumbers,
                Manufacturer = part.Manufacturer?.Name ?? string.Empty,
                ManufacturerPartNumber = part.ManufacturerProductNumber,
                TotalCost = (decimal)part.UnitPrice * barcodeResponse.Quantity,
                Cost = (decimal)part.UnitPrice,
                Currency = currency,
                DatasheetUrls = new List<string> { part.DatasheetUrl ?? string.Empty },
                Description = part.Description.ProductDescription + "\r\n" + part.Description.DetailedDescription,
                ImageUrl = part.PhotoUrl,
                PackageType = packageType,
                MountingTypeId = (int)mountingTypeId,
                PartType = "",
                ProductUrl = part.ProductUrl,
                Status = part.ProductStatus.Status,
                QuantityAvailable = barcodeResponse.Quantity,
                Quantity = barcodeResponse.Quantity,
            };
        }
    }
}
