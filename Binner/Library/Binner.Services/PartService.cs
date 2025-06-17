using AutoMapper;
using Binner.Common.Extensions;
using Binner.Services.Integrations;
using Binner.Global.Common;
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
using V3 = Binner.Model.Integrations.DigiKey.V3;
using V4 = Binner.Model.Integrations.DigiKey.V4;
using Binner.Model.Integrations;
using Binner.Common;
using Binner.Common.Integrations;

namespace Binner.Services
{
    public class PartService : IPartService
    {
        private const string MissingDatasheetCoverName = "datasheetcover.png";
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;

        private readonly WebHostServiceConfiguration _configuration;
        private readonly IStorageProvider _storageProvider;
        private readonly IMapper _mapper;
        private readonly IIntegrationApiFactory _integrationApiFactory;
        private readonly IRequestContextAccessor _requestContext;
        private readonly ILogger<PartService> _logger;
        private readonly IPartTypesCache _partTypesCache;

        public PartService(WebHostServiceConfiguration configuration, ILogger<PartService> logger, IStorageProvider storageProvider, IMapper mapper, IIntegrationApiFactory integrationApiFactory, IRequestContextAccessor requestContextAccessor, IPartTypesCache partTypesCache)
        {
            _configuration = configuration;
            _logger = logger;
            _storageProvider = storageProvider;
            _mapper = mapper;
            _integrationApiFactory = integrationApiFactory;
            _requestContext = requestContextAccessor;
            _partTypesCache = partTypesCache;
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
            else if (!string.IsNullOrEmpty(request.ShortId))
                return await _storageProvider.GetPartByShortIdAsync(request.ShortId, _requestContext.GetUserContext());
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

        public async Task<Part?> UpdatePartAsync(Part part)
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

        public async Task<PartType?> GetPartTypeAsync(long partTypeId)
        {
            return await _storageProvider.GetPartTypeAsync(partTypeId, _requestContext.GetUserContext());
        }

        public async Task<PartType?> GetPartTypeAsync(string name)
        {
            return await _storageProvider.GetPartTypeAsync(name, _requestContext.GetUserContext());
        }

        public async Task<ICollection<PartType>> GetPartTypesAsync()
        {
            return await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
        }

        public async Task<ICollection<PartType>> GetPartTypesAsync(bool filterEmpty)
        {
            return await _storageProvider.GetPartTypesAsync(filterEmpty, _requestContext.GetUserContext());
        }

        public Task<ICollection<PartTypeResponse>> GetPartTypesWithPartCountsAsync()
        {
            var partTypes = _partTypesCache.Cache
                .OrderBy(x => x.ParentPartType != null ? x.ParentPartType.Name : "")
                .ThenBy(x => x.Name)
                .ToList();
            return Task.FromResult(_mapper.Map<ICollection<PartTypeResponse>>(partTypes));
        }

        public async Task<ICollection<Part>> GetPartsByPartTypeAsync(PartType partType)
        {
            return await _storageProvider.GetPartsByPartTypeAsync(partType, _requestContext.GetUserContext());
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
                if (apiResponse.Response is V3.CategoriesResponse)
                {
                    var digikeyResponse = (V3.CategoriesResponse)apiResponse.Response;
                    return ServiceResult<CategoriesResponse?>.Create(_mapper.Map<CategoriesResponse>(digikeyResponse));
                } else if (apiResponse.Response is V4.CategoriesResponse)
                {
                    var digikeyResponse = (V4.CategoriesResponse)apiResponse.Response;
                    return ServiceResult<CategoriesResponse?>.Create(_mapper.Map<CategoriesResponse>(digikeyResponse));
                }
            }

            return ServiceResult<CategoriesResponse?>.Create("Invalid response received", apiResponse.ApiName);
        }

        private async Task<List<CommonPart>> MapCommonPartIdsAsync(List<CommonPart> parts)
        {
            var partNumbers = parts.Select(x => x.ManufacturerPartNumber ?? string.Empty).ToList();
            var partIds = await _storageProvider.GetPartIdsFromManufacturerPartNumbersAsync(partNumbers, _requestContext.GetUserContext());
            foreach (var part in parts)
            {
                var key = part.ManufacturerPartNumber ?? string.Empty;
                if (partIds.ContainsKey(key))
                    part.PartId = partIds[key];
            }
            return parts;
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

            if (apiResponse.Response is V3.OrderSearchResponse)
                return await ProcessDigiKeyV3OrderResponseAsync(digikeyApi, apiResponse, request);
            if (apiResponse.Response is V4.SalesOrder)
                return await ProcessDigiKeyV4OrderResponseAsync(digikeyApi, apiResponse, request);
            throw new InvalidOperationException();
        }

        private async Task<IServiceResult<ExternalOrderResponse?>> ProcessDigiKeyV4OrderResponseAsync(Integrations.DigikeyApi digikeyApi, IApiResponse apiResponse, OrderImportRequest request)
        {
            // for testing purposes
            //var fakeResultStr = @"{""orderId"":null,""supplier"":null,""orderDate"":""2025-02-02T01:41:48.109-08:00"",""amount"":758.5600000000001,""currency"":""USD"",""customerId"":""6983372"",""trackingNumber"":""731620024942"",""messages"":[],""parts"":[{""rank"":0,""partId"":1,""key"":""12ba15db-43a3-4fd6-a63e-0ad7b18a5bd6"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""1276-6455-2-ND"",""basePartNumber"":""CL21A106KOQNNNG"",""additionalPartNumbers"":[""1276-6455-6"",""1276-6455-2"",""1276-6455-1"",""CL21A106KOQNNNG-ND""],""manufacturer"":""Samsung Electro-Mechanics"",""manufacturerPartNumber"":""CL21A106KOQNNNG"",""cost"":0.0264,""totalCost"":13.2,""currency"":""USD"",""description"":""CAP CER 10UF 16V X5R 0805\r\n10 µF ±10% 16V Ceramic Capacitor X5R 0805 (2012 Metric)"",""datasheetUrls"":[""https://mm.digikey.com/Volume0/opasdata/d220001/medias/docus/658/CL21A106KOQNNNG_Spec.pdf""],""partType"":""Capacitor"",""partTypeId"":2,""mountingTypeId"":2,""keywords"":[""capacitor"",""ic"",""cl21a106koqnnng"",""cap"",""cer"",""10uf"",""16v"",""1276-6455-6"",""1276-6455-2"",""1276-6455-1"",""cl21a106koqnnng-nd"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/2537/Ceramic-Capacitor-CL-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/samsung-electro-mechanics/CL21A106KOQNNNG/3894417"",""packageType"":""0805 (2012 Metric)"",""status"":""Active"",""quantityAvailable"":1289997,""quantity"":500,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB C10"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""651b3870-ce0e-404d-99ce-884596c020b9"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""4713-GMC21X7R103J50NTTR-ND"",""basePartNumber"":""GMC21X7R103J50NT"",""additionalPartNumbers"":[""4713-GMC21X7R103J50NTDKR"",""4713-GMC21X7R103J50NTCT"",""4713-GMC21X7R103J50NTTR""],""manufacturer"":""Cal-Chip Electronics, Inc."",""manufacturerPartNumber"":""GMC21X7R103J50NT"",""cost"":0.0289,""totalCost"":2.89,""currency"":""USD"",""description"":""CAP0805 X7R .01UF 5% 50V\r\n10000 pF ±5% 50V Ceramic Capacitor X7R 0805 (2012 Metric)"",""datasheetUrls"":[""https://calchip.com/wp-content/uploads/2023/09/GMC-Series-2.pdf""],""partType"":""Capacitor"",""partTypeId"":2,""mountingTypeId"":2,""keywords"":[""capacitor"",""ic"",""gmc21x7r103j50nt"",""cap0805"",""x7r"","".01uf"",""5%"",""4713-gmc21x7r103j50ntdkr"",""4713-gmc21x7r103j50ntct"",""4713-gmc21x7r103j50nttr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/5781/MFG_Cal-Chip-All-Capacitors.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/cal-chip-electronics-inc/GMC21X7R103J50NT/22577164"",""packageType"":""0805 (2012 Metric)"",""status"":""Active"",""quantityAvailable"":1367,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB C11"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""5a1526ec-ddd7-4104-a3c3-9e8b61b3217e"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""1276-6780-2-ND"",""basePartNumber"":""CL21A226MOCLRNC"",""additionalPartNumbers"":[""1276-6780-1"",""1276-6780-2"",""1276-6780-6""],""manufacturer"":""Samsung Electro-Mechanics"",""manufacturerPartNumber"":""CL21A226MOCLRNC"",""cost"":0.08558,""totalCost"":42.79,""currency"":""USD"",""description"":""CAP CER 22UF 16V X5R 0805\r\n22 µF ±20% 16V Ceramic Capacitor X5R 0805 (2012 Metric)"",""datasheetUrls"":[""https://mm.digikey.com/Volume0/opasdata/d220001/medias/docus/43/CL21A226MOCLRNC_Spec.pdf""],""partType"":""Capacitor"",""partTypeId"":2,""mountingTypeId"":2,""keywords"":[""capacitor"",""ic"",""cl21a226moclrnc"",""cap"",""cer"",""22uf"",""16v"",""1276-6780-1"",""1276-6780-2"",""1276-6780-6"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/2537/Ceramic-Capacitor-CL-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/samsung-electro-mechanics/CL21A226MOCLRNC/5961264"",""packageType"":""0805 (2012 Metric)"",""status"":""Active"",""quantityAvailable"":3005210,""quantity"":500,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB C14-C17"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""ded4a109-59cd-4ae4-a993-9828ecde5a5b"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""1801-TSUP10M45SHTR-ND"",""basePartNumber"":""TSUP10M45SH"",""additionalPartNumbers"":[""TSUP10"",""1801-TSUP10M45SHTR"",""1801-TSUP10M45SHCT"",""1801-TSUP10M45SHS1GTR"",""1801-TSUP10M45SHS1GTR-ND"",""TSUP10M45SH S2G"",""1801-TSUP10M45SHDKR""],""manufacturer"":""Taiwan Semiconductor Corporation"",""manufacturerPartNumber"":""TSUP10M45SH"",""cost"":0.68,""totalCost"":13.6,""currency"":""USD"",""description"":""DIODE SCHOTTKY 45V 10A SMPC4.6U\r\nDiode 45 V 10A Surface Mount SMPC4.6U"",""datasheetUrls"":[""https://services.taiwansemi.com/storage/resources/datasheet/TSUP10M45SH_B2103.pdf""],""partType"":""Diode"",""partTypeId"":4,""mountingTypeId"":2,""keywords"":[""diode"",""ic"",""schottky"",""mount"",""tsup10m45sh"",""45v"",""10a"",""smpc4.6u"",""45"",""tsup10"",""1801-tsup10m45shtr"",""1801-tsup10m45shct"",""1801-tsup10m45shs1gtr"",""1801-tsup10m45shs1gtr-nd"",""tsup10m45sh s2g"",""1801-tsup10m45shdkr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/209/1801~SMPC4%2C6U~~3.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/taiwan-semiconductor-corporation/TSUP10M45SH/10669511"",""packageType"":""SMPC4.6U"",""status"":""Active"",""quantityAvailable"":10026,""quantity"":20,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB D1 - TSP10H45 ALT"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""be5c07c3-4d9f-42ca-9d60-fd11daf151be"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""18-SMF4L24CATR-ND"",""basePartNumber"":""SMF4L24CA"",""additionalPartNumbers"":[""18-SMF4L24CACT"",""18-SMF4L24CATR"",""18-SMF4L24CADKR""],""manufacturer"":""Littelfuse Inc."",""manufacturerPartNumber"":""SMF4L24CA"",""cost"":0.158,""totalCost"":15.8,""currency"":""USD"",""description"":""TVS DIODE 24VWM 38.9VC SOD123F\r\n38.9V Clamp 10.3A Ipp Tvs Diode Surface Mount SOD-123F"",""datasheetUrls"":[""https://www.littelfuse.com/assetdocs/tvs-diodes-smf4l-datasheet?assetguid=d2ba8fab-1de3-4176-8530-f36ecb0b8799""],""partType"":""Diode"",""partTypeId"":4,""mountingTypeId"":2,""keywords"":[""diode"",""mount"",""smf4l24ca"",""tvs"",""24vwm"",""38.9vc"",""sod123f"",""18-smf4l24cact"",""18-smf4l24catr"",""18-smf4l24cadkr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/6166/18~SOD123F~~2.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/littelfuse-inc/SMF4L24CA/10710255"",""packageType"":""SOD-123F"",""status"":""Active"",""quantityAvailable"":99235,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB D2"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""0e98bb64-59f4-40e7-92a9-1d7cd26837a2"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""SRP6540-1R0MTR-ND"",""basePartNumber"":""SRP6540-1R0M"",""additionalPartNumbers"":[""SRP6540"",""SRP6540-1R0MDKR"",""SRP6540-1R0MCT"",""Q9754313"",""SRP6540-1R0MTR""],""manufacturer"":""Bourns Inc."",""manufacturerPartNumber"":""SRP6540-1R0M"",""cost"":0.926,""totalCost"":23.15,""currency"":""USD"",""description"":""FIXED IND 1UH 12A 7.2 MOHM SMD\r\n1 µH Shielded Drum Core, Wirewound Inductor 12 A 7.2mOhm Max Nonstandard"",""datasheetUrls"":[""https://www.bourns.com/docs/Product-Datasheets/SRP6540.pdf""],""partType"":""Inductor"",""partTypeId"":3,""mountingTypeId"":2,""keywords"":[""inductor"",""srp6540-1r0m"",""fixed"",""ind"",""1uh"",""12a"",""srp6540"",""srp6540-1r0mdkr"",""srp6540-1r0mct"",""q9754313"",""srp6540-1r0mtr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/536/SRP6540-SERIES.JPG"",""productUrl"":""https://www.digikey.jp/en/products/detail/bourns-inc/SRP6540-1R0M/3986091"",""packageType"":""-"",""status"":""Active"",""quantityAvailable"":48648,""quantity"":25,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB L1"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""96ab4b40-bed3-41c1-ba45-a001cc90f5ad"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""31-AP7375-50W5-7TR-ND"",""basePartNumber"":""AP7375-50W5-7"",""additionalPartNumbers"":[""AP7375"",""31-AP7375-50W5-7TR"",""31-AP7375-50W5-7DKR"",""31-AP7375-50W5-7CT""],""manufacturer"":""Diodes Incorporated"",""manufacturerPartNumber"":""AP7375-50W5-7"",""cost"":0.33,""totalCost"":8.25,""currency"":""USD"",""description"":""IC REG LINEAR 5V 300MA SOT-25\r\nLinear Voltage Regulator IC Positive Fixed 1 Output 300mA SOT-25"",""datasheetUrls"":[""https://www.diodes.com/assets/Datasheets/AP7375.pdf""],""partType"":""Diode"",""partTypeId"":4,""mountingTypeId"":2,""keywords"":[""diode"",""ic"",""ap7375-50w5-7"",""reg"",""linear"",""5v"",""300ma"",""ap7375"",""31-ap7375-50w5-7tr"",""31-ap7375-50w5-7dkr"",""31-ap7375-50w5-7ct"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/6285/31~SOT25~DA%2CHR%2CHT%2CW~5.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/diodes-incorporated/AP7375-50W5-7/16400217"",""packageType"":""SOT-25"",""status"":""Active"",""quantityAvailable"":12787,""quantity"":25,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB U2"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""956b538c-6eee-4e05-99fc-bfbd9ff8aba3"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""785-AOZ2261BQI-28TR-ND"",""basePartNumber"":""AOZ2261BQI-28"",""additionalPartNumbers"":[""AOZ2261"",""785-AOZ2261BQI-28CT"",""785-AOZ2261BQI-28TR"",""785-AOZ2261BQI-28DKR""],""manufacturer"":""Alpha & Omega Semiconductor Inc."",""manufacturerPartNumber"":""AOZ2261BQI-28"",""cost"":0.5809,""totalCost"":58.09,""currency"":""USD"",""description"":""IC REG BUCK ADJ 8A 23QFNB\r\nBuck Switching Regulator IC Positive Adjustable 0.8V 1 Output 8A 23-PowerTFQFN"",""datasheetUrls"":[""https://aosmd.com/sites/default/files/res/datasheets/AOZ2261BQI-28.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""switch"",""aoz2261bqi-28"",""reg"",""buck"",""adj"",""8a"",""aoz2261"",""785-aoz2261bqi-28ct"",""785-aoz2261bqi-28tr"",""785-aoz2261bqi-28dkr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/2991/785~PO-00214~~23.JPG"",""productUrl"":""https://www.digikey.jp/en/products/detail/alpha-omega-semiconductor-inc/AOZ2261BQI-28/22248844"",""packageType"":""23-QFNB (4x4)"",""status"":""Active"",""quantityAvailable"":2890,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB U7"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""53c43835-f486-44bc-a70d-6970e297efd3"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""AP2120N-3.3TRG1DITR-ND"",""basePartNumber"":""AP2120N-3.3TRG1"",""additionalPartNumbers"":[""AP2120"",""AP2120N-3.3TRG1DITR"",""AP2120N-3.3TRG1DICT"",""AP2120N-3.3TRG1DI-ND"",""AP2120N-3.3TRG1DI"",""AP2120N-3.3TRG1DIDKR""],""manufacturer"":""Diodes Incorporated"",""manufacturerPartNumber"":""AP2120N-3.3TRG1"",""cost"":0.1138,""totalCost"":28.45,""currency"":""USD"",""description"":""IC REG LINEAR 3.3V 150MA SOT23-3\r\nLinear Voltage Regulator IC Positive Fixed 1 Output 150mA SOT-23-3"",""datasheetUrls"":[""https://www.diodes.com/assets/Datasheets/products_inactive_data/AP2120.pdf""],""partType"":""Diode"",""partTypeId"":4,""mountingTypeId"":2,""keywords"":[""diode"",""ic"",""ap2120n-3.3trg1"",""reg"",""linear"",""3.3v"",""150ma"",""ap2120"",""ap2120n-3.3trg1ditr"",""ap2120n-3.3trg1dict"",""ap2120n-3.3trg1di-nd"",""ap2120n-3.3trg1di"",""ap2120n-3.3trg1didkr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/4806/31~SOT23-3~F%2CN%2CSA~3.JPG"",""productUrl"":""https://www.digikey.jp/en/products/detail/diodes-incorporated/AP2120N-3-3TRG1/4470762"",""packageType"":""SOT-23-3"",""status"":""Active"",""quantityAvailable"":912638,""quantity"":250,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER CABINET U1"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""26ca3cdd-fb3d-49ef-9427-ff83bb8aab17"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""296-21931-2-ND"",""basePartNumber"":""TXS0102DCUR"",""additionalPartNumbers"":[""TXS0102"",""-296-21931-1"",""296-21931-2"",""296-21931-1"",""-TXS0102DCURG4-NDR"",""-TXS0102DCUR-NDR"",""296-21931-6"",""-296-21931-1-ND"",""-TXS0102DCURG4""],""manufacturer"":""Texas Instruments"",""manufacturerPartNumber"":""TXS0102DCUR"",""cost"":0.3031,""totalCost"":30.31,""currency"":""USD"",""description"":""IC TRANSLTR BIDIRECTIONAL 8VSSOP\r\nVoltage Level Translator Bidirectional 1 Circuit 2 Channel 24Mbps 8-VSSOP"",""datasheetUrls"":[""https://www.ti.com/general/docs/suppproductinfo.tsp?distId=10&gotoUrl=https%3A%2F%2Fwww.ti.com%2Flit%2Fgpn%2Ftxs0102""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""txs0102dcur"",""transltr"",""bidirectional"",""8vssop"",""voltage"",""txs0102"",""-296-21931-1"",""296-21931-2"",""296-21931-1"",""-txs0102dcurg4-ndr"",""-txs0102dcur-ndr"",""296-21931-6"",""-296-21931-1-nd"",""-txs0102dcurg4"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/6222/296~4200503~DCU~8.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/texas-instruments/TXS0102DCUR/1629104"",""packageType"":""8-VSSOP"",""status"":""Active"",""quantityAvailable"":9840,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB U7"",""partSupplierId"":null},{""rank"":0,""partId"":2,""key"":""7400b9c3-a234-4945-b37e-c4f7f3899770"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""ATTINY402-SSNRTR-ND"",""basePartNumber"":""ATTINY402-SSNR"",""additionalPartNumbers"":[""ATTINY402"",""ATTINY402-SSNRDKR"",""ATTINY402-SSNRTR"",""ATTINY402-SSNRCT""],""manufacturer"":""Microchip Technology"",""manufacturerPartNumber"":""ATTINY402-SSNR"",""cost"":0.43,""totalCost"":43,""currency"":""USD"",""description"":""IC MCU 8BIT 4KB FLASH 8SOIC\r\nAVR tinyAVR™ 0, Functional Safety (FuSa) Microcontroller IC 8-Bit 20MHz 4KB (4K x 8) FLASH 8-SOIC"",""datasheetUrls"":[""http://ww1.microchip.com/downloads/en/DeviceDoc/ATtiny202-402-AVR-MCU-with-Core-Independent-Peripherals_and-picoPower-40001969A.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""pic"",""microcontroller"",""attiny402-ssnr"",""mcu"",""8bit"",""4kb"",""flash"",""attiny402"",""attiny402-ssnrdkr"",""attiny402-ssnrtr"",""attiny402-ssnrct"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/4818/150%3B-8S1%3B-%3B-8.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/microchip-technology/ATTINY402-SSNR/9554946"",""packageType"":""8-SOIC"",""status"":""Active"",""quantityAvailable"":4510,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""0ec0fd76-93c5-41c0-9040-501443ffc2e4"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""732-5316-ND"",""basePartNumber"":""61300311121"",""additionalPartNumbers"":[""732-5316"",""3937-61300311121""],""manufacturer"":""Würth Elektronik"",""manufacturerPartNumber"":""61300311121"",""cost"":0.0712,""totalCost"":3.56,""currency"":""USD"",""description"":""CONN HEADER VERT 3POS 2.54MM\r\nConnector Header Through Hole 3 position 0.100\"" (2.54mm)"",""datasheetUrls"":[""https://www.we-online.com/components/products/datasheet/61300311121.pdf""],""partType"":""Connector"",""partTypeId"":13,""mountingTypeId"":1,""keywords"":[""connector"",""61300311121"",""conn"",""header"",""vert"",""3pos"",""732-5316"",""3937-61300311121"",""throughhole""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/1006/61300311121.JPG"",""productUrl"":""https://www.digikey.jp/en/products/detail/würth-elektronik/61300311121/4846825"",""packageType"":null,""status"":""Active"",""quantityAvailable"":125363,""quantity"":50,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""4ca2d060-c52e-4b1e-9ab5-5e71673e8593"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""490-14467-2-ND"",""basePartNumber"":""GRM21BR71E225KE11L"",""additionalPartNumbers"":[""GRM21BR71E"",""490-14467-6"",""490-14467-2"",""490-14467-1""],""manufacturer"":""Murata Electronics"",""manufacturerPartNumber"":""GRM21BR71E225KE11L"",""cost"":0.0504,""totalCost"":5.04,""currency"":""USD"",""description"":""CAP CER 2.2UF 25V X7R 0805\r\n2.2 µF ±10% 25V Ceramic Capacitor X7R 0805 (2012 Metric)"",""datasheetUrls"":[""https://search.murata.co.jp/Ceramy/image/img/A01X/G101/ENG/GRM21BR71E225KE11-01.pdf""],""partType"":""Capacitor"",""partTypeId"":2,""mountingTypeId"":2,""keywords"":[""capacitor"",""ic"",""grm21br71e225ke11l"",""cap"",""cer"",""2.2uf"",""25v"",""grm21br71e"",""490-14467-6"",""490-14467-2"",""490-14467-1"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/5447/490-Ceramic-Capacitor-Tan-Field.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/murata-electronics/GRM21BR71E225KE11L/6606096"",""packageType"":""0805 (2012 Metric)"",""status"":""Active"",""quantityAvailable"":1229145,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""4f97d3f8-184c-4a62-a6aa-a913898c71bf"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""311-2.20KCRTR-ND"",""basePartNumber"":""RC0805FR-072K2L"",""additionalPartNumbers"":[""RC0805FR072K2L"",""232273462202L"",""311-2.20KCRDKR"",""311-2.20KCRCT"",""311-2.20KCRTR"",""Q2717832G"",""9C08052A2201FKPFT""],""manufacturer"":""YAGEO"",""manufacturerPartNumber"":""RC0805FR-072K2L"",""cost"":0.0107,""totalCost"":1.07,""currency"":""USD"",""description"":""RES 2.2K OHM 1% 1/8W 0805\r\n2.2 kOhms ±1% 0.125W, 1/8W Chip Resistor 0805 (2012 Metric) Moisture Resistant Thick Film"",""datasheetUrls"":[""https://www.yageo.com/upload/media/product/products/datasheet/rchip/PYu-RC_Group_51_RoHS_L_12.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""resistor"",""rc0805fr-072k2l"",""res"",""2.2k"",""ohm"",""1%"",""rc0805fr072k2l"",""232273462202l"",""311-2.20kcrdkr"",""311-2.20kcrct"",""311-2.20kcrtr"",""q2717832g"",""9c08052a2201fkpft"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/4848/13_0805-%282012-metric%29.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/yageo/RC0805FR-072K2L/727676"",""packageType"":""0805"",""status"":""Active"",""quantityAvailable"":220746,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""53484892-9118-4595-8120-0d227df47b57"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""RMCF0805JT2K20TR-ND"",""basePartNumber"":""RMCF0805JT2K20"",""additionalPartNumbers"":[""RMCF0805JT2K20TR"",""RMCF0805JT2K20-ND"",""RMCF1/102.2K5%R-ND"",""RMCF1/102.2K5%RCT-ND"",""RMCF1/102.2K5%RTR"",""Q4955507V"",""RMCF0805JT2K20DKR"",""RMCF1/102.2K5%RDKR"",""RMCF1/102.2KJRCT"",""RMC1/102.2K5%R-ND"",""RMCF1/102.2KJRTR"",""RMC1/102.2KJR"",""RMCF1/102.2K5%RDKR-ND"",""RMCF1/102.2KJRDKR-ND"",""RMC1/102.2K5%R"",""RMCF1/102.2K5%RCT"",""RMCF1/102.2K5%R"",""RMCF0805JT2K20INACTIVE"",""RMC1/102.2KJR-ND"",""RMCF 1/10 2.2K 5% R"",""RMC 1/10 2.2K 5% R"",""RMCF1/102.2KJRDKR"",""RMCF1/102.2K5%RTR-ND"",""RMCF1/102.2KJRCT-ND"",""RMCF1/102.2KJRTR-ND"",""RMCF0805JT2K20CT""],""manufacturer"":""Stackpole Electronics Inc"",""manufacturerPartNumber"":""RMCF0805JT2K20"",""cost"":0.0056,""totalCost"":2.8,""currency"":""USD"",""description"":""RES 2.2K OHM 5% 1/8W 0805\r\n2.2 kOhms ±5% 0.125W, 1/8W Chip Resistor 0805 (2012 Metric) Automotive AEC-Q200 Thick Film"",""datasheetUrls"":[""https://www.seielect.com/catalog/sei-rmcf_rmcp.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""resistor"",""rmcf0805jt2k20"",""res"",""2.2k"",""ohm"",""5%"",""rmcf0805jt2k20tr"",""rmcf0805jt2k20-nd"",""rmcf1/102.2k5%r-nd"",""rmcf1/102.2k5%rct-nd"",""rmcf1/102.2k5%rtr"",""q4955507v"",""rmcf0805jt2k20dkr"",""rmcf1/102.2k5%rdkr"",""rmcf1/102.2kjrct"",""rmc1/102.2k5%r-nd"",""rmcf1/102.2kjrtr"",""rmc1/102.2kjr"",""rmcf1/102.2k5%rdkr-nd"",""rmcf1/102.2kjrdkr-nd"",""rmc1/102.2k5%r"",""rmcf1/102.2k5%rct"",""rmcf1/102.2k5%r"",""rmcf0805jt2k20inactive"",""rmc1/102.2kjr-nd"",""rmcf 1/10 2.2k 5% r"",""rmc 1/10 2.2k 5% r"",""rmcf1/102.2kjrdkr"",""rmcf1/102.2k5%rtr-nd"",""rmcf1/102.2kjrct-nd"",""rmcf1/102.2kjrtr-nd"",""rmcf0805jt2k20ct"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/639/0805-RMCF-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/stackpole-electronics-inc/RMCF0805JT2K20/1757894"",""packageType"":""0805"",""status"":""Active"",""quantityAvailable"":199328,""quantity"":500,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""47267a8c-bc75-4381-837e-4ddfc3348f56"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""568-1835-2-ND"",""basePartNumber"":""PCA9536D,118"",""additionalPartNumbers"":[""PCA9536"",""568-1835-6"",""935277415118"",""2832-PCA9536D,118TR"",""PCA9536D-T"",""568-1835-1"",""568-1835-2"",""PCA9536D118""],""manufacturer"":""NXP USA Inc."",""manufacturerPartNumber"":""PCA9536D,118"",""cost"":1.31,""totalCost"":2.62,""currency"":""USD"",""description"":""IC XPNDR 400KHZ I2C SMBUS 8SO\r\nI/O Expander 4 I2C, SMBus 400 kHz 8-SO"",""datasheetUrls"":[""https://www.nxp.com/docs/en/data-sheet/PCA9536.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""pca9536d,118"",""xpndr"",""400khz"",""i2c"",""smbus"",""pca9536"",""568-1835-6"",""935277415118"",""2832-pca9536d,118tr"",""pca9536d-t"",""568-1835-1"",""568-1835-2"",""pca9536d118"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/5345/568~SOT96-1~D%2CS%2CT%2CTN2~8.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/nxp-usa-inc/PCA9536D-118/789924"",""packageType"":""8-SO"",""status"":""Active"",""quantityAvailable"":24912,""quantity"":2,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""dfc4cdcb-5994-457f-b3bf-bf92ffc46d1d"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""296-13105-2-ND"",""basePartNumber"":""PCF8574ADWR"",""additionalPartNumbers"":[""PCF8574"",""296-13105-2"",""296-13105-1"",""2156-PCF8574ADWR"",""-PCF8574ADWRG4"",""296-13105-6"",""-296-13105-1-ND"",""-PCF8574ADWRE4"",""-PCF8574ADWRG4-NDR"",""-PCF8574ADWR-NDR"",""-296-13105-1"",""TEXTISPCF8574ADWR"",""-PCF8574ADWRE4-NDR""],""manufacturer"":""Texas Instruments"",""manufacturerPartNumber"":""PCF8574ADWR"",""cost"":1.17,""totalCost"":2.34,""currency"":""USD"",""description"":""IC XPNDR 100KHZ I2C 16SOIC\r\nI/O Expander 8 I2C 100 kHz 16-SOIC"",""datasheetUrls"":[""https://www.ti.com/lit/ds/symlink/pcf8574a.pdf?ts=1728869550852&ref_url=https%253A%252F%252Fwww.mouser.com%252F""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""pcf8574adwr"",""xpndr"",""100khz"",""i2c"",""16soic"",""pcf8574"",""296-13105-2"",""296-13105-1"",""2156-pcf8574adwr"",""-pcf8574adwrg4"",""296-13105-6"",""-296-13105-1-nd"",""-pcf8574adwre4"",""-pcf8574adwrg4-ndr"",""-pcf8574adwr-ndr"",""-296-13105-1"",""textispcf8574adwr"",""-pcf8574adwre4-ndr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/55/296~4221009~DW~16.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/texas-instruments/PCF8574ADWR/484754"",""packageType"":""16-SOIC"",""status"":""Active"",""quantityAvailable"":3179,""quantity"":2,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""2501ba79-3716-4cd1-a336-765c44908314"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""MCP23017-E/SS-ND"",""basePartNumber"":""MCP23017-E/SS"",""additionalPartNumbers"":[""MCP23017"",""MCP23017ESS""],""manufacturer"":""Microchip Technology"",""manufacturerPartNumber"":""MCP23017-E/SS"",""cost"":1.69,""totalCost"":3.38,""currency"":""USD"",""description"":""IC XPNDR 1.7MHZ I2C 28SSOP\r\nI/O Expander 16 I2C 1.7 MHz 28-SSOP"",""datasheetUrls"":[""https://ww1.microchip.com/downloads/aemDocuments/documents/APID/ProductDocuments/DataSheets/MCP23017-Data-Sheet-DS20001952.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""mcp23017-e/ss"",""xpndr"",""1.7mhz"",""i2c"",""28ssop"",""mcp23017"",""mcp23017ess"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/4183/150~C04-073~SS~28.JPG"",""productUrl"":""https://www.digikey.jp/en/products/detail/microchip-technology/MCP23017-E-SS/894273"",""packageType"":""28-SSOP"",""status"":""Active"",""quantityAvailable"":14667,""quantity"":2,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""f2fc87fd-ad02-4f32-a554-6b06eafe9fc1"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""1276-1029-2-ND"",""basePartNumber"":""CL21B105KBFNNNE"",""additionalPartNumbers"":[""1276-1029-1"",""1276-1029-2"",""1276-1029-6""],""manufacturer"":""Samsung Electro-Mechanics"",""manufacturerPartNumber"":""CL21B105KBFNNNE"",""cost"":0.0321,""totalCost"":3.21,""currency"":""USD"",""description"":""CAP CER 1UF 50V X7R 0805\r\n1 µF ±10% 50V Ceramic Capacitor X7R 0805 (2012 Metric)"",""datasheetUrls"":[""https://mm.digikey.com/Volume0/opasdata/d220001/medias/docus/609/CL21B105KBFNNNE_Spec.pdf""],""partType"":""Capacitor"",""partTypeId"":2,""mountingTypeId"":2,""keywords"":[""capacitor"",""ic"",""cl21b105kbfnnne"",""cap"",""cer"",""1uf"",""50v"",""1276-1029-1"",""1276-1029-2"",""1276-1029-6"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/2537/Ceramic-Capacitor-CL-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/samsung-electro-mechanics/CL21B105KBFNNNE/3886687"",""packageType"":""0805 (2012 Metric)"",""status"":""Active"",""quantityAvailable"":523997,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB C4,C8"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""4c69b3c6-5779-4439-8ac1-84b1a399d089"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""RMCF0805ZT0R00TR-ND"",""basePartNumber"":""RMCF0805ZT0R00"",""additionalPartNumbers"":[""RMCF1/100RTR-ND"",""RMCF1/100RDKR"",""RMCF0805ZT0R00DKR"",""Q4955507X"",""RMCF1/100RCT-ND"",""RMCF1/100RTR"",""RMCF0805ZT0R00CT"",""RMCF1/100R-ND"",""RMCF0805ZT0R00TR"",""RMCF1/100R"",""RMCF 1/10 0 R"",""RMCF1/100RDKR-ND"",""RMCF1/100RCT""],""manufacturer"":""Stackpole Electronics Inc"",""manufacturerPartNumber"":""RMCF0805ZT0R00"",""cost"":0.00473,""totalCost"":4.73,""currency"":""USD"",""description"":""RES 0 OHM JUMPER 1/8W 0805\r\n0 Ohms Jumper Chip Resistor 0805 (2012 Metric) Automotive AEC-Q200 Thick Film"",""datasheetUrls"":[""https://www.seielect.com/catalog/sei-rmcf_rmcp.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""resistor"",""rmcf0805zt0r00"",""res"",""0"",""ohm"",""jumper"",""rmcf1/100rtr-nd"",""rmcf1/100rdkr"",""rmcf0805zt0r00dkr"",""q4955507x"",""rmcf1/100rct-nd"",""rmcf1/100rtr"",""rmcf0805zt0r00ct"",""rmcf1/100r-nd"",""rmcf0805zt0r00tr"",""rmcf1/100r"",""rmcf 1/10 0 r"",""rmcf1/100rdkr-nd"",""rmcf1/100rct"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/639/0805-RMCF-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/stackpole-electronics-inc/RMCF0805ZT0R00/1756901"",""packageType"":""0805"",""status"":""Active"",""quantityAvailable"":2025564,""quantity"":1000,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""bc079147-7b25-47c4-bf4b-35d74200745c"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""455-SM05B-SRSS-TBTR-ND"",""basePartNumber"":""SM05B-SRSS-TB"",""additionalPartNumbers"":[""SM05B-SR"","""",""SM05B-SRSS-TB(LF)(SN)"",""455-SM05B-SRSS-TBCT"",""SM05BSRSSTBLFSN"",""455-1805-6-ND"",""SM05BSRSSTB"",""455-SM05B-SRSS-TBDKR"",""455-1805-1-ND"",""455-1805-2-ND"",""455-1805-6"",""(G)SM05B-SRSS-TB(LF)(SN)"",""455-1805-1"",""455-1805-2"",""455-SM05B-SRSS-TBTR""],""manufacturer"":""JST Sales America Inc."",""manufacturerPartNumber"":""SM05B-SRSS-TB"",""cost"":0.4239,""totalCost"":42.39,""currency"":""USD"",""description"":""CONN HEADER SMD R/A 5POS 1MM\r\nConnector Header Surface Mount, Right Angle 5 position 0.039\"" (1.00mm)"",""datasheetUrls"":[""https://www.jst-mfg.com/product/pdf/eng/eSH.pdf""],""partType"":""Connector"",""partTypeId"":13,""mountingTypeId"":2,""keywords"":[""connector"",""mount"",""sm05b-srss-tb"",""conn"",""header"",""smd"",""r/a"",""sm05b-sr"","""",""sm05b-srss-tb(lf)(sn)"",""455-sm05b-srss-tbct"",""sm05bsrsstblfsn"",""455-1805-6-nd"",""sm05bsrsstb"",""455-sm05b-srss-tbdkr"",""455-1805-1-nd"",""455-1805-2-nd"",""455-1805-6"",""(g)sm05b-srss-tb(lf)(sn)"",""455-1805-1"",""455-1805-2"",""455-sm05b-srss-tbtr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/865/SM05B-SRSS-TB%28LF%29%28SN%29.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/jst-sales-america-inc/SM05B-SRSS-TB/926711"",""packageType"":null,""status"":""Active"",""quantityAvailable"":27149,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB DISPLAY J8"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""4a367dd2-672d-46c0-aba2-15d723a6e15d"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""455-BM05B-SRSS-TBTR-ND"",""basePartNumber"":""BM05B-SRSS-TB"",""additionalPartNumbers"":[""BM05B-SR"",""BM05BSRSSTBLFSN"","""",""(G)BM05B-SRSS-TB(LF)(SN)"",""455-1409-2-ND"",""455-1791-6"",""455-BM05B-SRSS-TBCT"",""455-1791-1"",""455-BM05B-SRSS-TBTR"",""455-BM05B-SRSS-TBDKR"",""455-1791-2-ND"",""455-1791-2"",""455-1791-6-ND"",""BM05BSRSSTB"",""455-1409-2"",""BM05B-SRSS-TB(LF)(SN)"",""455-1791-1-ND""],""manufacturer"":""JST Sales America Inc."",""manufacturerPartNumber"":""BM05B-SRSS-TB"",""cost"":0.4118,""totalCost"":41.18,""currency"":""USD"",""description"":""CONN HEADER SMD 5POS 1MM\r\nConnector Header Surface Mount 5 position 0.039\"" (1.00mm)"",""datasheetUrls"":[""https://mm.digikey.com/Volume0/opasdata/d220001/medias/docus/5562/SH Connector.pdf""],""partType"":""Connector"",""partTypeId"":13,""mountingTypeId"":2,""keywords"":[""connector"",""mount"",""bm05b-srss-tb"",""conn"",""header"",""smd"",""5pos"",""bm05b-sr"",""bm05bsrsstblfsn"","""",""(g)bm05b-srss-tb(lf)(sn)"",""455-1409-2-nd"",""455-1791-6"",""455-bm05b-srss-tbct"",""455-1791-1"",""455-bm05b-srss-tbtr"",""455-bm05b-srss-tbdkr"",""455-1791-2-nd"",""455-1791-2"",""455-1791-6-nd"",""bm05bsrsstb"",""455-1409-2"",""bm05b-srss-tb(lf)(sn)"",""455-1791-1-nd"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/990/BM05B-SRSS-TB%28LF%29%28SN%29.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/jst-sales-america-inc/BM05B-SRSS-TB/926697"",""packageType"":null,""status"":""Active"",""quantityAvailable"":226810,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB DISPLAY J1"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""9038c734-af55-4c86-bb3b-2bf187091680"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""455-S3B-XH-A-ND"",""basePartNumber"":""S3B-XH-A"",""additionalPartNumbers"":[""S3B-XH-A"",""455-S3B-XH-A"",""S3B-XH-A(LF)(SN)"",""(G)S3B-XH-A(LF)(SN)"",""S3BXHALFSN"",""S3B-XH-ALFSN"",""455-2250-ND"",""455-2250""],""manufacturer"":""JST Sales America Inc."",""manufacturerPartNumber"":""S3B-XH-A"",""cost"":0.0924,""totalCost"":18.48,""currency"":""USD"",""description"":""CONN HEADER R/A 3POS 2.5MM\r\nConnector Header Through Hole, Right Angle 3 position 0.098\"" (2.50mm)"",""datasheetUrls"":[""https://www.jst-mfg.com/product/pdf/eng/eXH.pdf""],""partType"":""Connector"",""partTypeId"":13,""mountingTypeId"":1,""keywords"":[""connector"",""s3b-xh-a"",""conn"",""header"",""r/a"",""3pos"",""455-s3b-xh-a"",""s3b-xh-a(lf)(sn)"",""(g)s3b-xh-a(lf)(sn)"",""s3bxhalfsn"",""s3b-xh-alfsn"",""455-2250-nd"",""455-2250"",""throughhole""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/705/S3B-XH-A%28LF%29%28SN%29.JPG"",""productUrl"":""https://www.digikey.jp/en/products/detail/jst-sales-america-inc/S3B-XH-A/1651048"",""packageType"":null,""status"":""Active"",""quantityAvailable"":128161,""quantity"":200,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CAB J6,J7"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""5c7521e7-53ed-4a4f-a0c3-a9121b4d03b9"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""RMCF0805FT10R0TR-ND"",""basePartNumber"":""RMCF0805FT10R0"",""additionalPartNumbers"":[""RMCF0805FT10R0CT"",""RMCF1/10101%RCT-ND"",""RMCF1/1010FRTR"",""RMCF0805FT10R0-ND"",""Q6979215W"",""RMCF1/1010FRDKR"",""RMCF1/10101%RDKR"",""RMCF1/1010FRCT-ND"",""RMCF1/10101%R"",""RMCF1/10101%RTR"",""RMCF1/1010FRTR-ND"",""RMCF 1/10 10 1% R"",""RMCF1/10101%R-ND"",""RMCF1/1010FRCT"",""RMCF0805FT10R0TR"",""RMCF1/10101%RCT"",""RMCF1/10101%RTR-ND"",""RMCF1/1010FRDKR-ND"",""RMCF1/10101%RDKR-ND"",""RMCF0805FT10R0DKR""],""manufacturer"":""Stackpole Electronics Inc"",""manufacturerPartNumber"":""RMCF0805FT10R0"",""cost"":0.00602,""totalCost"":6.02,""currency"":""USD"",""description"":""RES 10 OHM 1% 1/8W 0805\r\n10 Ohms ±1% 0.125W, 1/8W Chip Resistor 0805 (2012 Metric) Automotive AEC-Q200 Thick Film"",""datasheetUrls"":[""https://www.seielect.com/catalog/sei-rmcf_rmcp.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""resistor"",""rmcf0805ft10r0"",""res"",""10"",""ohm"",""1%"",""rmcf0805ft10r0ct"",""rmcf1/10101%rct-nd"",""rmcf1/1010frtr"",""rmcf0805ft10r0-nd"",""q6979215w"",""rmcf1/1010frdkr"",""rmcf1/10101%rdkr"",""rmcf1/1010frct-nd"",""rmcf1/10101%r"",""rmcf1/10101%rtr"",""rmcf1/1010frtr-nd"",""rmcf 1/10 10 1% r"",""rmcf1/10101%r-nd"",""rmcf1/1010frct"",""rmcf0805ft10r0tr"",""rmcf1/10101%rct"",""rmcf1/10101%rtr-nd"",""rmcf1/1010frdkr-nd"",""rmcf1/10101%rdkr-nd"",""rmcf0805ft10r0dkr"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/639/0805-RMCF-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/stackpole-electronics-inc/RMCF0805FT10R0/1760155"",""packageType"":""0805"",""status"":""Active"",""quantityAvailable"":269477,""quantity"":1000,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER CONTROLLER"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""dbabd875-2fc0-4a8c-be63-7b5074b0e6b0"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""RMCF0805FT51K0TR-ND"",""basePartNumber"":""RMCF0805FT51K0"",""additionalPartNumbers"":[""RMC1/1051KFR"",""-RMCF0805FT51K0CT"",""RMCF0805FT51K0DKR"",""-RMCF0805FT51K0TR"",""RMC1/1051KFR-ND"",""RMCF0805FT51K0TR"",""RMC1/1051K1%R-ND"",""RMCF0805FT51K0CT"",""RMC 1/10 51K 1% R"",""RMC1/1051K1%R"",""RMCF0805FT51K0-ND""],""manufacturer"":""Stackpole Electronics Inc"",""manufacturerPartNumber"":""RMCF0805FT51K0"",""cost"":0.00696,""totalCost"":3.48,""currency"":""USD"",""description"":""RES 51K OHM 1% 1/8W 0805\r\n51 kOhms ±1% 0.125W, 1/8W Chip Resistor 0805 (2012 Metric) Automotive AEC-Q200 Thick Film"",""datasheetUrls"":[""https://www.seielect.com/catalog/sei-rmcf_rmcp.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""resistor"",""rmcf0805ft51k0"",""res"",""51k"",""ohm"",""1%"",""rmc1/1051kfr"",""-rmcf0805ft51k0ct"",""rmcf0805ft51k0dkr"",""-rmcf0805ft51k0tr"",""rmc1/1051kfr-nd"",""rmcf0805ft51k0tr"",""rmc1/1051k1%r-nd"",""rmcf0805ft51k0ct"",""rmc 1/10 51k 1% r"",""rmc1/1051k1%r"",""rmcf0805ft51k0-nd"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/639/0805-RMCF-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/stackpole-electronics-inc/RMCF0805FT51K0/1713219"",""packageType"":""0805"",""status"":""Active"",""quantityAvailable"":120733,""quantity"":500,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":"""",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""a715d64d-1934-4324-ab7e-d9f296888da5"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""399-13672-2-ND"",""basePartNumber"":""A765EB107M1CLAE025"",""additionalPartNumbers"":[""A765EB"",""399-13672-6"",""399-13672-2"",""EA765EB107M1CLA"",""399-13672-1""],""manufacturer"":""KEMET"",""manufacturerPartNumber"":""A765EB107M1CLAE025"",""cost"":0.3692,""totalCost"":18.46,""currency"":""USD"",""description"":""CAP ALUM POLY 100UF 20% 16V SMD\r\n100 µF 16 V Aluminum - Polymer Capacitors Radial, Can - SMD 25mOhm 2000 Hrs @ 105°C"",""datasheetUrls"":[""https://search.kemet.com/download/datasheet/A765EB107M1CLAE025""],""partType"":""Capacitor"",""partTypeId"":2,""mountingTypeId"":2,""keywords"":[""capacitor"",""a765eb107m1clae025"",""cap"",""alum"",""poly"",""100uf"",""a765eb"",""399-13672-6"",""399-13672-2"",""ea765eb107m1cla"",""399-13672-1"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/2550/MFG_A765-series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/kemet/A765EB107M1CLAE025/6196340"",""packageType"":""Radial, Can - SMD"",""status"":""Active"",""quantityAvailable"":1520,""quantity"":50,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CONTROLLER C3"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""8dad44ee-bdce-419d-9a4c-99ea8730526c"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""118-CR0805-FX-7502ELFTR-ND"",""basePartNumber"":""CR0805-FX-7502ELF"",""additionalPartNumbers"":[""CR0805"",""-CR0805-FX-7502ELFTR"",""118-CR0805-FX-7502ELFDKR"",""CR0805-FX-7502ELF-ND"",""118-CR0805-FX-7502ELFCT"",""CR0805-FX-7502ELFCT"",""CR0805-FX-7502ELFTR-ND"",""CR0805-FX-7502ELFTR"",""CR0805-FX-7502ELFCT-ND"",""118-CR0805-FX-7502ELFTR"",""-CR0805-FX-7502ELFCT""],""manufacturer"":""Bourns Inc."",""manufacturerPartNumber"":""CR0805-FX-7502ELF"",""cost"":0.00728,""totalCost"":3.64,""currency"":""USD"",""description"":""RES SMD 75K OHM 1% 1/8W 0805\r\n75 kOhms ±1% 0.125W, 1/8W Chip Resistor 0805 (2012 Metric) Moisture Resistant Thick Film"",""datasheetUrls"":[""https://bourns.com/docs/product-datasheets/cr.pdf?sfvrsn=574d41f6_14""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""resistor"",""cr0805-fx-7502elf"",""res"",""smd"",""75k"",""ohm"",""cr0805"",""-cr0805-fx-7502elftr"",""118-cr0805-fx-7502elfdkr"",""cr0805-fx-7502elf-nd"",""118-cr0805-fx-7502elfct"",""cr0805-fx-7502elfct"",""cr0805-fx-7502elftr-nd"",""cr0805-fx-7502elftr"",""cr0805-fx-7502elfct-nd"",""118-cr0805-fx-7502elftr"",""-cr0805-fx-7502elfct"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/505/CR0805-%282012-Metric%29.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/bourns-inc/CR0805-FX-7502ELF/3785109"",""packageType"":""0805"",""status"":""Active"",""quantityAvailable"":709237,""quantity"":500,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER CAB R21"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""fba72bf2-4686-4960-b562-4433a9439831"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""RMCF0805FT390KTR-ND"",""basePartNumber"":""RMCF0805FT390K"",""additionalPartNumbers"":[""-RMCF0805FT390KCT"",""RMCF0805FT390KDKR"",""-RMCF0805FT390KDKR"",""RMC1/10390KFR"",""RMCF0805FT390KTR"",""RMC1/10390K1%R"",""RMCF0805FT390KCT"",""RMCF0805FT390K-ND"",""RMC1/10390K1%R-ND"",""RMC1/10390KFR-ND"",""-RMCF0805FT390KTR"",""RMC 1/10 390K 1% R""],""manufacturer"":""Stackpole Electronics Inc"",""manufacturerPartNumber"":""RMCF0805FT390K"",""cost"":0.00696,""totalCost"":3.48,""currency"":""USD"",""description"":""RES 390K OHM 1% 1/8W 0805\r\n390 kOhms ±1% 0.125W, 1/8W Chip Resistor 0805 (2012 Metric) Automotive AEC-Q200 Thick Film"",""datasheetUrls"":[""https://www.seielect.com/catalog/sei-rmcf_rmcp.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""resistor"",""rmcf0805ft390k"",""res"",""390k"",""ohm"",""1%"",""-rmcf0805ft390kct"",""rmcf0805ft390kdkr"",""-rmcf0805ft390kdkr"",""rmc1/10390kfr"",""rmcf0805ft390ktr"",""rmc1/10390k1%r"",""rmcf0805ft390kct"",""rmcf0805ft390k-nd"",""rmc1/10390k1%r-nd"",""rmc1/10390kfr-nd"",""-rmcf0805ft390ktr"",""rmc 1/10 390k 1% r"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/639/0805-RMCF-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/stackpole-electronics-inc/RMCF0805FT390K/1713078"",""packageType"":""0805"",""status"":""Active"",""quantityAvailable"":22845,""quantity"":500,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER CAB R20"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""14c6fe79-30fd-4986-bda3-54d337d9778d"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""1N4148W-FDITR-ND"",""basePartNumber"":""1N4148W-7-F"",""additionalPartNumbers"":[""1N4148"",""1N4148W-FDIDKR"",""1N4148W7F"",""1N4148W-FDITR"",""1N4148W-FDICT"",""IN4148W-7-F""],""manufacturer"":""Diodes Incorporated"",""manufacturerPartNumber"":""1N4148W-7-F"",""cost"":0.0395,""totalCost"":7.9,""currency"":""USD"",""description"":""DIODE STANDARD 100V 300MA SOD123\r\nDiode 100 V 300mA Surface Mount SOD-123"",""datasheetUrls"":[""https://www.diodes.com/assets/Datasheets/BAV16W_1N4148W.pdf""],""partType"":""Diode"",""partTypeId"":4,""mountingTypeId"":2,""keywords"":[""diode"",""mount"",""1n4148w-7-f"",""standard"",""100v"",""300ma"",""sod123"",""1n4148"",""1n4148w-fdidkr"",""1n4148w7f"",""1n4148w-fditr"",""1n4148w-fdict"",""in4148w-7-f"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/5753/31~SOD123~~2.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/diodes-incorporated/1N4148W-7-F/814371"",""packageType"":""SOD-123"",""status"":""Active"",""quantityAvailable"":467826,""quantity"":200,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER D7"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""3dd094a7-eb35-4749-8e44-b418c17bf973"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""CKN12319-2-ND"",""basePartNumber"":""PTS636SM50JSMTR LFS"",""additionalPartNumbers"":[""PTS636"",""PTS636 SM50J SMTR LFS"",""CKN12319-1"",""CKN12319-2"",""CKN12319-6"",""PTS636SK43JSMTRLFS""],""manufacturer"":""C&K"",""manufacturerPartNumber"":""PTS636SM50JSMTR LFS"",""cost"":0.1635,""totalCost"":16.35,""currency"":""USD"",""description"":""SWITCH TACTILE SPST-NO 0.05A 12V\r\nTactile Switch SPST-NO Top Actuated Surface Mount"",""datasheetUrls"":[""https://www.ckswitches.com/media/2779/pts636.pdf""],""partType"":""Switch"",""partTypeId"":11,""mountingTypeId"":2,""keywords"":[""switch"",""mount"",""pts636sm50jsmtr lfs"",""tactile"",""spst-no"",""0.05a"",""12v"",""pts636"",""pts636 sm50j smtr lfs"",""ckn12319-1"",""ckn12319-2"",""ckn12319-6"",""pts636sk43jsmtrlfs"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/316/MFG_PTS636-J-Lead.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/c-k/PTS636SM50JSMTR-LFS/10071732"",""packageType"":null,""status"":""Active"",""quantityAvailable"":15854,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER DISPLAY"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""f66071ae-e262-483e-83f4-fe541ff3f8ac"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""497-1201-2-ND"",""basePartNumber"":""L78M05ABDT-TR"",""additionalPartNumbers"":[""L78M05"",""L78M05ABDTTR"",""497-1201-6"",""5060-L78M05ABDT-TR"",""497-1201-1"",""497-1201-2"",""497-1201-2-NDR"",""497-1201-1-NDR""],""manufacturer"":""STMicroelectronics"",""manufacturerPartNumber"":""L78M05ABDT-TR"",""cost"":0.275,""totalCost"":2.75,""currency"":""USD"",""description"":""IC REG LINEAR 5V 500MA DPAK\r\nLinear Voltage Regulator IC Positive Fixed 1 Output 500mA DPAK"",""datasheetUrls"":[""https://mm.digikey.com/Volume0/opasdata/d220001/medias/docus/2578/L78MxxAB%2CAC .pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""l78m05abdt-tr"",""reg"",""linear"",""5v"",""500ma"",""l78m05"",""l78m05abdttr"",""497-1201-6"",""5060-l78m05abdt-tr"",""497-1201-1"",""497-1201-2"",""497-1201-2-ndr"",""497-1201-1-ndr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/6256/497~DPAK%28TO252-3%29~~2.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/stmicroelectronics/L78M05ABDT-TR/585725"",""packageType"":""DPAK"",""status"":""Active"",""quantityAvailable"":51604,""quantity"":10,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""5e06259a-3a37-406e-8254-ec6ba545a0c2"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""296-17616-2-ND"",""basePartNumber"":""UA78M05IDCYR"",""additionalPartNumbers"":[""UA78M05"",""296-17616-6"",""296-17616-2-NDR"",""-296-17616-1-NDR"",""-296-17616-1-ND"",""-296-17616-1"",""-UA78M05IDCYRG3"",""296-17616-6-NDR"",""2156-UA78M05IDCYR"",""TEXTISUA78M05IDCYR"",""296-17616-2"",""296-17616-1-NDR"",""-UA78M05IDCYR-NDR"",""-UA78M05IDCYRG3-NDR"",""296-17616-1""],""manufacturer"":""Texas Instruments"",""manufacturerPartNumber"":""UA78M05IDCYR"",""cost"":0.2967,""totalCost"":29.67,""currency"":""USD"",""description"":""IC REG LINEAR 5V 500MA SOT-223-4\r\nLinear Voltage Regulator IC Positive Fixed 1 Output 500mA SOT-223-4"",""datasheetUrls"":[""https://www.ti.com/lit/ds/slvs059t/slvs059t.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""ua78m05idcyr"",""reg"",""linear"",""5v"",""500ma"",""ua78m05"",""296-17616-6"",""296-17616-2-ndr"",""-296-17616-1-ndr"",""-296-17616-1-nd"",""-296-17616-1"",""-ua78m05idcyrg3"",""296-17616-6-ndr"",""2156-ua78m05idcyr"",""textisua78m05idcyr"",""296-17616-2"",""296-17616-1-ndr"",""-ua78m05idcyr-ndr"",""-ua78m05idcyrg3-ndr"",""296-17616-1"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/6222/296~4202506~DCY~4.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/texas-instruments/UA78M05IDCYR/706596"",""packageType"":""SOT-223-4"",""status"":""Active"",""quantityAvailable"":8572,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER CAB U2 NEW"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""a31bf861-27c4-4c6a-8304-7e1b8b35119d"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""1276-2959-2-ND"",""basePartNumber"":""CL21B334KBFNFNE"",""additionalPartNumbers"":[""1276-2959-1"",""1276-2959-2"",""1276-2959-6""],""manufacturer"":""Samsung Electro-Mechanics"",""manufacturerPartNumber"":""CL21B334KBFNFNE"",""cost"":0.0356,""totalCost"":3.56,""currency"":""USD"",""description"":""CAP CER 0.33UF 50V X7R 0805\r\n0.33 µF ±10% 50V Ceramic Capacitor X7R 0805 (2012 Metric)"",""datasheetUrls"":[""http://www.samsungsem.com/kr/support/product-search/mlcc/CL21B334KBFNFNE.jsp""],""partType"":""Capacitor"",""partTypeId"":2,""mountingTypeId"":2,""keywords"":[""capacitor"",""ic"",""cl21b334kbfnfne"",""cap"",""cer"",""0.33uf"",""50v"",""1276-2959-1"",""1276-2959-2"",""1276-2959-6"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/2537/Ceramic-Capacitor-CL-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/samsung-electro-mechanics/CL21B334KBFNFNE/3888617"",""packageType"":""0805 (2012 Metric)"",""status"":""Active"",""quantityAvailable"":119737,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""FOR UA78M05"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""6417ceae-85c4-4166-800b-45bfdd6312d3"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""150-ATTINY3226-MURTR-ND"",""basePartNumber"":""ATTINY3226-MUR"",""additionalPartNumbers"":[""ATTINY3226"",""150-ATTINY3226-MURCT"",""150-ATTINY3226-MURDKR"",""150-ATTINY3226-MURTR""],""manufacturer"":""Microchip Technology"",""manufacturerPartNumber"":""ATTINY3226-MUR"",""cost"":0.88,""totalCost"":88,""currency"":""USD"",""description"":""IC MCU 8BIT 32KB FLASH 20VQFN\r\nAVR tinyAVR® 2 Microcontroller IC 8-Bit 20MHz 32KB (32K x 8) FLASH 20-VQFN (3x3)"",""datasheetUrls"":[""https://ww1.microchip.com/downloads/aemDocuments/documents/MCU08/ProductDocuments/DataSheets/ATtiny3224-3226-3227-Data-Sheet-DS40002345B.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""microcontroller"",""attiny3226-mur"",""mcu"",""8bit"",""32kb"",""flash"",""attiny3226"",""150-attiny3226-murct"",""150-attiny3226-murdkr"",""150-attiny3226-murtr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/6094/150~C04-21380~REB~20.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/microchip-technology/ATTINY3226-MUR/17631082"",""packageType"":""20-VQFN (3x3)"",""status"":""Active"",""quantityAvailable"":706,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER CAB U1"",""partSupplierId"":null},{""rank"":0,""partId"":3,""key"":""22cd6e80-12f1-4511-ab93-029c9ea6ca9f"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""150-ATTINY3226-SFRTR-ND"",""basePartNumber"":""ATTINY3226-SFR"",""additionalPartNumbers"":[""ATTINY3226"",""150-ATTINY3226-SFRCT"",""150-ATTINY3226-SFRDKR"",""150-ATTINY3226-SFRTR""],""manufacturer"":""Microchip Technology"",""manufacturerPartNumber"":""ATTINY3226-SFR"",""cost"":1.45,""totalCost"":7.25,""currency"":""USD"",""description"":""IC MCU 8BIT 32KB FLASH 20SOIC\r\nAVR tinyAVR® 2 Microcontroller IC 8-Bit 20MHz 32KB (32K x 8) FLASH 20-SOIC"",""datasheetUrls"":[""https://ww1.microchip.com/downloads/aemDocuments/documents/MCU08/ProductDocuments/DataSheets/ATtiny3224-3226-3227-Data-Sheet-DS40002345B.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""microcontroller"",""attiny3226-sfr"",""mcu"",""8bit"",""32kb"",""flash"",""attiny3226"",""150-attiny3226-sfrct"",""150-attiny3226-sfrdkr"",""150-attiny3226-sfrtr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/5397/150~C04-094~SO~20.JPG"",""productUrl"":""https://www.digikey.jp/en/products/detail/microchip-technology/ATTINY3226-SFR/17631046"",""packageType"":""20-SOIC"",""status"":""Active"",""quantityAvailable"":1201,""quantity"":5,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER STOCK"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""6499880b-662d-4240-aac1-ffde1023d4e5"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""RMCF0805FT1K50TR-ND"",""basePartNumber"":""RMCF0805FT1K50"",""additionalPartNumbers"":[""RMCF1/101.5K1%RDKR"",""RMCF1/101.5KFRTR-ND"",""RMCF1/101.5KFRCT-ND"",""RMCF 1/10 1.5K 1% R"",""RMCF1/101.5K1%R-ND"",""RMCF1/101.5KFRCT"",""RMCF1/101.5K1%RTR-ND"",""RMCF1/101.5K1%RCT-ND"",""RMCF0805FT1K50CT"",""RMCF0805FT1K50-ND"",""RMCF1/101.5K1%RCT"",""RMCF1/101.5K1%RDKR-ND"",""RMCF0805FT1K50DKR"",""RMCF1/101.5KFRDKR-ND"",""RMCF1/101.5KFRTR"",""RMCF1/101.5KFRDKR"",""RMCF1/101.5K1%R"",""RMCF0805FT1K50TR"",""RMCF1/101.5K1%RTR""],""manufacturer"":""Stackpole Electronics Inc"",""manufacturerPartNumber"":""RMCF0805FT1K50"",""cost"":0.00696,""totalCost"":3.48,""currency"":""USD"",""description"":""RES 1.5K OHM 1% 1/8W 0805\r\n1.5 kOhms ±1% 0.125W, 1/8W Chip Resistor 0805 (2012 Metric) Automotive AEC-Q200 Thick Film"",""datasheetUrls"":[""https://www.seielect.com/catalog/sei-rmcf_rmcp.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""resistor"",""rmcf0805ft1k50"",""res"",""1.5k"",""ohm"",""1%"",""rmcf1/101.5k1%rdkr"",""rmcf1/101.5kfrtr-nd"",""rmcf1/101.5kfrct-nd"",""rmcf 1/10 1.5k 1% r"",""rmcf1/101.5k1%r-nd"",""rmcf1/101.5kfrct"",""rmcf1/101.5k1%rtr-nd"",""rmcf1/101.5k1%rct-nd"",""rmcf0805ft1k50ct"",""rmcf0805ft1k50-nd"",""rmcf1/101.5k1%rct"",""rmcf1/101.5k1%rdkr-nd"",""rmcf0805ft1k50dkr"",""rmcf1/101.5kfrdkr-nd"",""rmcf1/101.5kfrtr"",""rmcf1/101.5kfrdkr"",""rmcf1/101.5k1%r"",""rmcf0805ft1k50tr"",""rmcf1/101.5k1%rtr"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/639/0805-RMCF-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/stackpole-electronics-inc/RMCF0805FT1K50/1760652"",""packageType"":""0805"",""status"":""Active"",""quantityAvailable"":151642,""quantity"":500,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":"""",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""d10f6e48-b658-474b-a4ff-4ff5cc525901"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""RMCF0805FT1K10TR-ND"",""basePartNumber"":""RMCF0805FT1K10"",""additionalPartNumbers"":[""RMCF0805FT1K10TR"",""RMCF1/101.1K1%R-ND"",""RMCF1/101.1K1%RDKR-ND"",""RMCF1/101.1K1%RCT"",""RMCF1/101.1K1%RDKR"",""RMCF1/101.1KFRTR"",""RMCF1/101.1K1%RCT-ND"",""RMCF 1/10 1.1K 1% R"",""RMCF1/101.1K1%RTR-ND"",""RMCF1/101.1KFRTR-ND"",""RMCF1/101.1K1%RTR"",""RMCF0805FT1K10CT"",""RMCF1/101.1KFRDKR-ND"",""RMCF0805FT1K10DKR"",""RMCF1/101.1K1%R"",""RMCF1/101.1KFRDKR"",""RMCF0805FT1K10-ND"",""RMCF1/101.1KFRCT-ND"",""RMCF1/101.1KFRCT""],""manufacturer"":""Stackpole Electronics Inc"",""manufacturerPartNumber"":""RMCF0805FT1K10"",""cost"":0.00696,""totalCost"":3.48,""currency"":""USD"",""description"":""RES 1.1K OHM 1% 1/8W 0805\r\n1.1 kOhms ±1% 0.125W, 1/8W Chip Resistor 0805 (2012 Metric) Automotive AEC-Q200 Thick Film"",""datasheetUrls"":[""https://www.seielect.com/catalog/sei-rmcf_rmcp.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""resistor"",""rmcf0805ft1k10"",""res"",""1.1k"",""ohm"",""1%"",""rmcf0805ft1k10tr"",""rmcf1/101.1k1%r-nd"",""rmcf1/101.1k1%rdkr-nd"",""rmcf1/101.1k1%rct"",""rmcf1/101.1k1%rdkr"",""rmcf1/101.1kfrtr"",""rmcf1/101.1k1%rct-nd"",""rmcf 1/10 1.1k 1% r"",""rmcf1/101.1k1%rtr-nd"",""rmcf1/101.1kfrtr-nd"",""rmcf1/101.1k1%rtr"",""rmcf0805ft1k10ct"",""rmcf1/101.1kfrdkr-nd"",""rmcf0805ft1k10dkr"",""rmcf1/101.1k1%r"",""rmcf1/101.1kfrdkr"",""rmcf0805ft1k10-nd"",""rmcf1/101.1kfrct-nd"",""rmcf1/101.1kfrct"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/639/0805-RMCF-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/stackpole-electronics-inc/RMCF0805FT1K10/1760460"",""packageType"":""0805"",""status"":""Active"",""quantityAvailable"":282191,""quantity"":500,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":"""",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""4c15ad5b-c22a-4eb6-a30f-791743923cdf"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""1276-1244-2-ND"",""basePartNumber"":""CL21A475KAQNNNE"",""additionalPartNumbers"":[""1276-1244-1"",""1276-1244-2"",""1276-1244-6""],""manufacturer"":""Samsung Electro-Mechanics"",""manufacturerPartNumber"":""CL21A475KAQNNNE"",""cost"":0.03,""totalCost"":9,""currency"":""USD"",""description"":""CAP CER 4.7UF 25V X5R 0805\r\n4.7 µF ±10% 25V Ceramic Capacitor X5R 0805 (2012 Metric)"",""datasheetUrls"":[""https://mm.digikey.com/Volume0/opasdata/d220001/medias/docus/41/CL21A475KAQNNNE_Spec.pdf""],""partType"":""Capacitor"",""partTypeId"":2,""mountingTypeId"":2,""keywords"":[""capacitor"",""ic"",""cl21a475kaqnnne"",""cap"",""cer"",""4.7uf"",""25v"",""1276-1244-1"",""1276-1244-2"",""1276-1244-6"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/2537/Ceramic-Capacitor-CL-Series.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/samsung-electro-mechanics/CL21A475KAQNNNE/3886902"",""packageType"":""0805 (2012 Metric)"",""status"":""Active"",""quantityAvailable"":2425749,""quantity"":300,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER C10"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""9a9148f3-e567-4575-ad7c-132743fc38c6"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""AZ1117CH-3.3TRG1DITR-ND"",""basePartNumber"":""AZ1117CH-3.3TRG1"",""additionalPartNumbers"":[""AZ1117"",""AZ1117CH-3.3TRG1DIDKR"",""AZ1117CH-3.3TRG1DICT"",""AZ1117CH-3.3TRG1DITR"",""AZ1117CH-3.3TRG1DI-ND"",""AZ1117CH-3.3TRG1DI""],""manufacturer"":""Diodes Incorporated"",""manufacturerPartNumber"":""AZ1117CH-3.3TRG1"",""cost"":0.13012,""totalCost"":32.53,""currency"":""USD"",""description"":""IC REG LINEAR 3.3V 1A SOT-223-3\r\nLinear Voltage Regulator IC Positive Fixed 1 Output 1A SOT-223-3"",""datasheetUrls"":[""https://www.diodes.com/assets/Datasheets/AZ1117C.pdf""],""partType"":""Diode"",""partTypeId"":4,""mountingTypeId"":2,""keywords"":[""diode"",""ic"",""az1117ch-3.3trg1"",""reg"",""linear"",""3.3v"",""1a"",""az1117"",""az1117ch-3.3trg1didkr"",""az1117ch-3.3trg1dict"",""az1117ch-3.3trg1ditr"",""az1117ch-3.3trg1di-nd"",""az1117ch-3.3trg1di"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/4895/31~SOT223~E%2CG%2CH~4.JPG"",""productUrl"":""https://www.digikey.jp/en/products/detail/diodes-incorporated/AZ1117CH-3-3TRG1/4470985"",""packageType"":""SOT-223-3"",""status"":""Active"",""quantityAvailable"":32341,""quantity"":250,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":"""",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""c9f72e1d-ae45-40a7-b59c-23891197de2d"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""NCV7805BDTRKGOSTR-ND"",""basePartNumber"":""NCV7805BDTRKG"",""additionalPartNumbers"":[""NCV7805"",""2156-NCV7805BDTRKG-OS"",""NCV7805BDTRKGOSDKR"",""ONSONSNCV7805BDTRKG"",""NCV7805BDTRKGOSTR"",""NCV7805BDTRKG-ND"",""NCV7805BDTRKGOSCT""],""manufacturer"":""onsemi"",""manufacturerPartNumber"":""NCV7805BDTRKG"",""cost"":0.5864,""totalCost"":14.66,""currency"":""USD"",""description"":""IC REG LINEAR 5V 1A DPAK\r\nLinear Voltage Regulator IC Positive Fixed 1 Output 1A DPAK"",""datasheetUrls"":[""https://www.onsemi.com/pdf/datasheet/mc7800-d.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""ncv7805bdtrkg"",""reg"",""linear"",""5v"",""1a"",""ncv7805"",""2156-ncv7805bdtrkg-os"",""ncv7805bdtrkgosdkr"",""onsonsncv7805bdtrkg"",""ncv7805bdtrkgostr"",""ncv7805bdtrkg-nd"",""ncv7805bdtrkgosct"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/5044/488~369C-01~D%2CDT~2.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/onsemi/NCV7805BDTRKG/1792758"",""packageType"":""DPAK"",""status"":""Active"",""quantityAvailable"":77358,""quantity"":25,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CONTROLLER U1"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""0820f5c7-70cf-4005-af03-c9af907589e3"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""296-22863-2-ND"",""basePartNumber"":""TXS0101DBVR"",""additionalPartNumbers"":[""TXS0101"",""TEXTISTXS0101DBVR"",""296-22863-6"",""296-22863-6-NDR"",""-TXS0101DBVRG4"",""-296-22863-1-ND"",""296-22863-1-NDR"",""-296-22863-1-NDR"",""-TXS0101DBVRG4-NDR"",""296-22863-2-NDR"",""-TXS0101DBVR-NDR"",""296-22863-1"",""296-22863-2"",""2156-TXS0101DBVR""],""manufacturer"":""Texas Instruments"",""manufacturerPartNumber"":""TXS0101DBVR"",""cost"":0.2495,""totalCost"":24.95,""currency"":""USD"",""description"":""IC TRANSLATOR BIDIR SOT23-6\r\nVoltage Level Translator Bidirectional 1 Circuit 1 Channel 24Mbps SOT-23-6"",""datasheetUrls"":[""https://www.ti.com/general/docs/suppproductinfo.tsp?distId=10&gotoUrl=https%3A%2F%2Fwww.ti.com%2Flit%2Fgpn%2Ftxs0101""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""txs0101dbvr"",""translator"",""bidir"",""sot23-6"",""voltage"",""txs0101"",""textistxs0101dbvr"",""296-22863-6"",""296-22863-6-ndr"",""-txs0101dbvrg4"",""-296-22863-1-nd"",""296-22863-1-ndr"",""-296-22863-1-ndr"",""-txs0101dbvrg4-ndr"",""296-22863-2-ndr"",""-txs0101dbvr-ndr"",""296-22863-1"",""296-22863-2"",""2156-txs0101dbvr"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/6222/296~4073253-5~DBV~6.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/texas-instruments/TXS0101DBVR/1739892"",""packageType"":""SOT-23-6"",""status"":""Active"",""quantityAvailable"":4767,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":"""",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""37b2ab2c-9ae3-4127-9665-c041febb40ee"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""DS2484R+TTR-ND"",""basePartNumber"":""DS2484R+T"",""additionalPartNumbers"":[""DS2484"",""DS2484R+TDKR"",""DS2484R+TCT"",""DS2484R+TTR"",""DS2484R+T-ND""],""manufacturer"":""Analog Devices Inc./Maxim Integrated"",""manufacturerPartNumber"":""DS2484R+T"",""cost"":2.227,""totalCost"":22.27,""currency"":""USD"",""description"":""IC MASTER I2C-1WIRE 1CH SOT-6\r\n1-Wire® Controller I2C Interface SOT-6"",""datasheetUrls"":[""https://www.analog.com/media/en/technical-documentation/data-sheets/ds2484.pdf""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":0,""keywords"":[""ic"",""ds2484r+t"",""master"",""i2c-1wire"",""1ch"",""sot-6"",""ds2484"",""ds2484r+tdkr"",""ds2484r+tct"",""ds2484r+ttr"",""ds2484r+t-nd"",""none""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/4277/175~21-0058~UT~6.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/analog-devices-inc-maxim-integrated/DS2484R-T/5020823"",""packageType"":""SOT-6"",""status"":""Active"",""quantityAvailable"":46,""quantity"":10,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER U6"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""79d71ae5-47ab-4dce-95c0-7f3e6137a94b"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""296-23770-2-ND"",""basePartNumber"":""INA219AIDCNR"",""additionalPartNumbers"":[""INA219"",""296-23770-2-NDR"",""2156-INA219AIDCNR"",""TEXTISINA219AIDCNR"",""-296-23770-1-NDR"",""-HPA00900AIDCNR"",""296-23770-6"",""-INA219AIDCNR-NDR"",""-INA219AIDCNRG4-NDR"",""296-23770-6-NDR"",""-HPA00900AIDCNR-NDR"",""296-23770-1-NDR"",""-296-23770-1-ND"",""-INA219AIDCNRG4"",""296-23770-1"",""296-23770-2""],""manufacturer"":""Texas Instruments"",""manufacturerPartNumber"":""INA219AIDCNR"",""cost"":1.669,""totalCost"":16.69,""currency"":""USD"",""description"":""IC CURRENT MONITOR 1% SOT23-8\r\nCurrent Monitor Regulator High-Side 10mA SOT-23-8"",""datasheetUrls"":[""https://www.ti.com/general/docs/suppproductinfo.tsp?distId=10&gotoUrl=https%3A%2F%2Fwww.ti.com%2Flit%2Fgpn%2Fina219""],""partType"":""IC"",""partTypeId"":14,""mountingTypeId"":2,""keywords"":[""ic"",""ina219aidcnr"",""current"",""monitor"",""1%"",""sot23-8"",""ina219"",""296-23770-2-ndr"",""2156-ina219aidcnr"",""textisina219aidcnr"",""-296-23770-1-ndr"",""-hpa00900aidcnr"",""296-23770-6"",""-ina219aidcnr-ndr"",""-ina219aidcnrg4-ndr"",""296-23770-6-ndr"",""-hpa00900aidcnr-ndr"",""296-23770-1-ndr"",""-296-23770-1-nd"",""-ina219aidcnrg4"",""296-23770-1"",""296-23770-2"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/6222/296~4202106~DCN~8.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/texas-instruments/INA219AIDCNR/1952540"",""packageType"":""SOT-23-8"",""status"":""Active"",""quantityAvailable"":66402,""quantity"":10,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER 24V CONTROLLER"",""partSupplierId"":null},{""rank"":0,""partId"":null,""key"":""20bffc73-64a4-422c-89b1-14d28dd3c8fa"",""swarmPartNumberManufacturerId"":null,""supplier"":""DigiKey"",""supplierPartNumber"":""455-SM03B-SRSS-TBTR-ND"",""basePartNumber"":""SM03B-SRSS-TB"",""additionalPartNumbers"":[""SM03B-SR"",""SM03BSRSSTB"",""455-SM03B-SRSS-TBCT"",""455-SM03B-SRSS-TBDKR"",""(G)SM03B-SRSS-TB(LF)(SN)(P)"",""455-1803-6-ND"",""455-1803-1"",""SM03BSRSSTBLFSNP"",""455-1803-1-ND"",""SM03B-SRSS-TB(LF)(SN)(P)"",""455-1803-6"",""SM03B-SRSS-TB(LF)(SN)"",""455-1803-2"",""455-SM03B-SRSS-TBTR"",""455-1803-2-ND""],""manufacturer"":""JST Sales America Inc."",""manufacturerPartNumber"":""SM03B-SRSS-TB"",""cost"":0.3061,""totalCost"":30.61,""currency"":""USD"",""description"":""CONN HEADER SMD R/A 3POS 1MM\r\nConnector Header Surface Mount, Right Angle 3 position 0.039\"" (1.00mm)"",""datasheetUrls"":[""https://www.jst-mfg.com/product/pdf/eng/eSH.pdf""],""partType"":""Connector"",""partTypeId"":13,""mountingTypeId"":2,""keywords"":[""connector"",""mount"",""sm03b-srss-tb"",""conn"",""header"",""smd"",""r/a"",""sm03b-sr"",""sm03bsrsstb"",""455-sm03b-srss-tbct"",""455-sm03b-srss-tbdkr"",""(g)sm03b-srss-tb(lf)(sn)(p)"",""455-1803-6-nd"",""455-1803-1"",""sm03bsrsstblfsnp"",""455-1803-1-nd"",""sm03b-srss-tb(lf)(sn)(p)"",""455-1803-6"",""sm03b-srss-tb(lf)(sn)"",""455-1803-2"",""455-sm03b-srss-tbtr"",""455-1803-2-nd"",""surfacemount""],""imageUrl"":""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/932/SM03B-SRSS-TB%28LF%29%28SN%29.jpg"",""productUrl"":""https://www.digikey.jp/en/products/detail/jst-sales-america-inc/SM03B-SRSS-TB/926709"",""packageType"":null,""status"":""Active"",""quantityAvailable"":19517,""quantity"":100,""minimumOrderQuantity"":0,""factoryStockAvailable"":null,""factoryLeadTime"":null,""reference"":""BINNER CABINET ESD J10-J13"",""partSupplierId"":null}]}";
            //var fakeObj = JsonConvert.DeserializeObject<ExternalOrderResponse>(fakeResultStr);
            //return ServiceResult<ExternalOrderResponse?>.Create(fakeObj);

            var messages = new List<Model.Responses.Message>();
            var digikeyOrderResponse = (V4.SalesOrder?)apiResponse.Response ?? new V4.SalesOrder();

            var lineItems = digikeyOrderResponse.LineItems;
            var commonParts = new List<CommonPart>();
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());

            var digikeyApiMaxOrderLineItems = 50;
            var isLargeOrder = lineItems.Count > digikeyApiMaxOrderLineItems;
            var currency = digikeyOrderResponse.Currency;
            // always use the currency of the order, as product details may be specified in the user's chosen currency in Binner
            if (string.IsNullOrEmpty(currency))
                currency = _configuration.Locale.Currency.ToString().ToUpper();

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
                if (string.IsNullOrEmpty(lineItem.DigiKeyProductNumber))
                    continue;

                if (!request.RequestProductInfo || lineItemCount > digikeyApiMaxOrderLineItems || errorsEncountered > 0)
                {
                    commonParts.Add(DigiKeyV4LineItemToCommonPart(lineItem, currency));
                    continue;
                }

                // for each line item, get the full product information from the product details api endpoint (max 50 requests).
                IApiResponse? partResponse = null;
                var productResponseSuccess = false;
                try
                {
                    partResponse = await digikeyApi.GetProductDetailsAsync(lineItem.DigiKeyProductNumber);
                    if (!partResponse.RequiresAuthentication && partResponse.Errors.Any() == false)
                        productResponseSuccess = true;
                    if (partResponse.RequiresAuthentication)
                    {
                        messages.Add(Model.Responses.Message.FromError($"Failed to get product details on part '{lineItem.DigiKeyProductNumber}'. Api: Requires authentication."));
                        errorsEncountered++;
                    }
                    if (partResponse.Errors.Any() == true)
                    {
                        messages.Add(Model.Responses.Message.FromError($"Failed to get product details on part '{lineItem.DigiKeyProductNumber}'. Api Errors: {string.Join(",", partResponse.Errors)}"));
                        errorsEncountered++;
                    }
                }
                catch (Exception ex)
                {
                    // likely we have been throttled
                    messages.Add(Model.Responses.Message.FromError($"Failed to get product details on part '{lineItem.DigiKeyProductNumber}'. Exception: {ex.GetBaseException().Message}"));
                    errorsEncountered++;
                }

                if (productResponseSuccess && partResponse?.Response != null)
                {
                    var details = (V4.ProductDetails)partResponse.Response;
                    if (details != null)
                    {
                        var part = (V4.Product?)details.Product;
                        if (part != null)
                        {
                            // convert the part to a common part
                            var additionalPartNumbers = new List<string>();
                            var basePart = part.Parameters
                                .Where(x => x.ParameterText.Equals("Base Part Number", ComparisonType))
                                .Select(x => x.ValueText)
                                .FirstOrDefault();

                            if (!string.IsNullOrEmpty(basePart))
                                additionalPartNumbers.Add(basePart);

                            if (!string.IsNullOrEmpty(part.BaseProductNumber?.Name))
                                additionalPartNumbers.Add(part.BaseProductNumber.Name);
                            if (part.OtherNames?.Any() == true)
                                additionalPartNumbers.AddRange(part.OtherNames);

                            if (string.IsNullOrEmpty(basePart))
                                basePart = part.ManufacturerProductNumber;

                            var mountingTypeString = part.Parameters
                                .Where(x => x.ParameterText.Equals("Mounting Type", ComparisonType))
                                .Select(x => x.ValueText?.Replace(" ", "") ?? string.Empty)
                                .FirstOrDefault();
                            if (mountingTypeString?.Contains("SurfaceMount", ComparisonType) == true) // some part types are comma delimited
                                mountingTypeString = "SurfaceMount";
                            if (mountingTypeString?.Contains("ThroughHole", ComparisonType) == true) // some part types are comma delimited
                                mountingTypeString = "ThroughHole";
                            Enum.TryParse<MountingType>(mountingTypeString, out var mountingTypeId);

                            var packageType = part.Parameters
                                    ?.Where(x => x.ParameterText.Equals("Supplier Device Package", ComparisonType))
                                    .Select(x => x.ValueText)
                                    .FirstOrDefault();
                            if (string.IsNullOrEmpty(packageType))
                                packageType = part.Parameters
                                    ?.Where(x => x.ParameterText.Equals("Package / Case", ComparisonType))
                                    .Select(x => x.ValueText)
                                    .FirstOrDefault();

                            commonParts.Add(DigiKeyV4PartToCommonPart(part, currency, additionalPartNumbers, basePart, (int)mountingTypeId, packageType, lineItem));
                        }
                    }
                }
                else
                {
                    messages.Add(Model.Responses.Message.FromInfo($"No additional product details available on part '{lineItem.DigiKeyProductNumber}'."));
                    // use the more minimal information provided by the order import call
                    commonParts.Add(DigiKeyV4LineItemToCommonPart(lineItem, currency));
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
                OrderDate = digikeyOrderResponse.DateEntered,
                Currency = digikeyOrderResponse.Currency,
                CustomerId = digikeyOrderResponse.CustomerId.ToString(),
                Amount = lineItems.Sum(x => x.TotalPrice),
                TrackingNumber = string.Join(",", lineItems.SelectMany(x => x.ItemShipments.Where(y => !string.IsNullOrEmpty(y.TrackingNumber)).Select(y => y.TrackingNumber)).Distinct()),
                Messages = messages,
                Parts = commonParts
            });
        }

        private async Task<IServiceResult<ExternalOrderResponse?>> ProcessDigiKeyV3OrderResponseAsync(Integrations.DigikeyApi digikeyApi, IApiResponse apiResponse, OrderImportRequest request)
        {
            var messages = new List<Model.Responses.Message>();
            var digikeyResponse = (V3.OrderSearchResponse?)apiResponse.Response ?? new V3.OrderSearchResponse();

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
                    commonParts.Add(DigiKeyV3LineItemToCommonPart(lineItem));
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
                    var part = (V3.Product?)partResponse.Response ?? new V3.Product();
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

                    commonParts.Add(DigiKeyV3PartToCommonPart(part, currency, additionalPartNumbers, basePart, (int)mountingTypeId, packageType, lineItem));
                }
                else
                {
                    messages.Add(Model.Responses.Message.FromInfo($"No additional product details available on part '{lineItem.DigiKeyPartNumber}'."));
                    // use the more minimal information provided by the order import call
                    commonParts.Add(DigiKeyV3LineItemToCommonPart(lineItem));
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
                OrderDate = digikeyResponse.ShippingDetails.Any() ? DateTime.Parse(digikeyResponse.ShippingDetails.First().DateTransaction ?? DateTime.MinValue.ToString()) : DateTime.MinValue,
                Currency = digikeyResponse.Currency,
                CustomerId = digikeyResponse.CustomerId.ToString(),
                Amount = lineItems.Sum(x => x.TotalPrice),
                TrackingNumber = digikeyResponse.ShippingDetails.Any() ? digikeyResponse.ShippingDetails.First().TrackingUrl : "",
                Messages = messages,
                Parts = commonParts
            });
        }

        private CommonPart DigiKeyV4PartToCommonPart(V4.Product part, string currency, ICollection<string> additionalPartNumbers, string? basePart, int mountingTypeId, string? packageType, V4.LineItem lineItem) => new CommonPart
        {
            SupplierPartNumber = part.ProductVariations.FirstOrDefault()?.DigiKeyProductNumber ?? string.Empty,
            Supplier = "DigiKey",
            ManufacturerPartNumber = part.ManufacturerProductNumber,
            Manufacturer = part.Manufacturer?.Name ?? string.Empty,
            Description = part.Description.ProductDescription + "\r\n" + part.Description.DetailedDescription,
            ImageUrl = SystemPaths.EnsureValidAbsoluteHttpUrl(part.PhotoUrl),
            DatasheetUrls = new List<string> { SystemPaths.EnsureValidAbsoluteHttpUrl(part.DatasheetUrl) ?? string.Empty },
            ProductUrl = SystemPaths.EnsureValidAbsoluteHttpUrl(part.ProductUrl),
            Status = part.ProductStatus.Status,
            Currency = currency, // currency should be from the order
            AdditionalPartNumbers = additionalPartNumbers,
            BasePartNumber = basePart,
            MountingTypeId = mountingTypeId,
            PackageType = packageType,
            Cost = lineItem.UnitPrice,  // cost should be from the order
            TotalCost = lineItem.TotalPrice,
            QuantityAvailable = part.QuantityAvailable, // qty should be from the order
            Quantity = lineItem.QuantityOrdered,  // qty should be from the order
            Reference = lineItem.CustomerReference,
        };

        private CommonPart DigiKeyV3PartToCommonPart(V3.Product part, string currency, ICollection<string> additionalPartNumbers, string? basePart, int mountingTypeId, string? packageType, V3.LineItem lineItem) => new CommonPart
        {
            SupplierPartNumber = part.DigiKeyPartNumber,
            Supplier = "DigiKey",
            ManufacturerPartNumber = part.ManufacturerPartNumber,
            Manufacturer = part.Manufacturer?.Value ?? string.Empty,
            Description = part.ProductDescription + "\r\n" + part.DetailedDescription,
            ImageUrl = SystemPaths.EnsureValidAbsoluteHttpUrl(part.PrimaryPhoto),
            DatasheetUrls = new List<string> { SystemPaths.EnsureValidAbsoluteHttpUrl(part.PrimaryDatasheet) ?? string.Empty },
            ProductUrl = SystemPaths.EnsureValidAbsoluteHttpUrl(part.ProductUrl),
            Status = part.ProductStatus,
            Currency = currency,    // currency should be from the order
            AdditionalPartNumbers = additionalPartNumbers,
            BasePartNumber = basePart,
            MountingTypeId = mountingTypeId,
            PackageType = packageType,
            Cost = lineItem.UnitPrice,  // cost should be from the order
            TotalCost = lineItem.TotalPrice,
            QuantityAvailable = part.QuantityAvailable,  // qty available is vendor stock
            Quantity = lineItem.Quantity,  // qty should be from the order
            Reference = lineItem.CustomerReference,
        };

        private CommonPart DigiKeyV4LineItemToCommonPart(V4.LineItem lineItem, string currency) => new CommonPart
        {
            SupplierPartNumber = lineItem.DigiKeyProductNumber,
            Supplier = "DigiKey",
            ManufacturerPartNumber = lineItem.ManufacturerProductNumber,
            Manufacturer = string.Empty,
            Description = lineItem.Description,
            Currency = currency,
            Cost = lineItem.UnitPrice,
            TotalCost = lineItem.TotalPrice,
            QuantityAvailable = lineItem.QuantityOrdered,
            Quantity = lineItem.QuantityOrdered,  // quantity ordered
            Reference = lineItem.CustomerReference,
        };

        private CommonPart DigiKeyV3LineItemToCommonPart(V3.LineItem lineItem) => new CommonPart
        {
            SupplierPartNumber = lineItem.DigiKeyPartNumber,
            Supplier = "DigiKey",
            ManufacturerPartNumber = string.Empty,
            Manufacturer = string.Empty,
            Description = lineItem.ProductDescription,
            Cost = lineItem.UnitPrice,
            TotalCost = lineItem.TotalPrice,
            QuantityAvailable = lineItem.Quantity,
            Quantity = lineItem.Quantity, // quantity ordered
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

        public async Task<IServiceResult<V3.Product?>> GetBarcodeInfoProductAsync(string barcode, ScannedLabelType barcodeType)
        {
            var user = _requestContext.GetUserContext();
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
            if (!digikeyApi.IsEnabled)
                return ServiceResult<V3.Product?>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            // currently only supports DigiKey, as Mouser barcodes are part numbers
            var response = new PartResults();
            var digikeyResponse = new ProductBarcodeResponse();
            if (digikeyApi.IsEnabled)
            {
                var apiResponse = await digikeyApi.GetBarcodeDetailsAsync(barcode, barcodeType);
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
                    var partResponse = await digikeyApi.GetProductDetailsAsync(digikeyResponse.DigiKeyPartNumber);
                    if (!partResponse.RequiresAuthentication && partResponse?.Errors.Any() == false)
                    {
                        var part = (V3.Product?)partResponse.Response;
                        return ServiceResult<V3.Product?>.Create(part);
                    }
                }
            }

            return ServiceResult<V3.Product?>.Create(null);
        }

        public async Task<IServiceResult<PartResults?>> GetBarcodeInfoAsync(string barcode, ScannedLabelType barcodeType)
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
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }
                response.Parts = await MapCommonPartIdsAsync(response.Parts);
            }

            return ServiceResult<PartResults>.Create(response);
        }

        private async Task<CommonPart> DigikeyV3ProductToCommonPartAsync(V3.Product part, ProductBarcodeResponse barcodeResponse)
        {
            // todo: unify this
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
            var result = new CommonPart
            {
                Supplier = "DigiKey",
                SupplierPartNumber = part.DigiKeyPartNumber,
                BasePartNumber = basePart ?? part.ManufacturerPartNumber,
                AdditionalPartNumbers = additionalPartNumbers,
                Manufacturer = part.Manufacturer?.Value ?? string.Empty,
                ManufacturerPartNumber = part.ManufacturerPartNumber,
                TotalCost = part.UnitPrice,
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
                ProductUrl = part.ProductUrl,
                Status = part.ProductStatus,
                QuantityAvailable = barcodeResponse.Quantity,
                Quantity = barcodeResponse.Quantity,
            };
            var partType = await DeterminePartTypeAsync(result);
            result.PartType = partType?.Name ?? string.Empty;
            result.PartTypeId = partType?.PartTypeId ?? 0;
            return result;
        }

        private CommonPart DigikeyV4ProductDetailsToCommonPart(V4.ProductDetails details, ProductBarcodeResponse barcodeResponse)
        {
            // todo: unify this
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
                    currency = _configuration.Locale.Currency.ToString().ToUpper();
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
                    TotalCost = part.UnitPrice * barcodeResponse.Quantity,
                    Cost = part.UnitPrice,
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
                            TotalCost = supplier.Cost ?? 0,
                            Cost = supplier.Cost ?? 0,
                            Currency = _configuration.Locale.Currency.ToString().ToUpper(),
                            ImageUrl = supplier.ImageUrl,
                            ProductUrl = supplier.ProductUrl,
                            QuantityAvailable = supplier.QuantityAvailable,
                            Quantity = supplier.QuantityAvailable,
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
            catch (ApiErrorException ex)
            {
                // fatal error with executing api request
                return ServiceResult<PartResults>.Create(ex.ApiResponse.Errors, ex.ApiResponse.ApiName);
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

            // map PartIds for local inventory
            response.Parts = await MapCommonPartIdsAsync(response.Parts);

            // Combine all the product images and datasheets into the root response and remove duplicates
            response.ProductImages = partInfoResults.PartResults.ProductImages.DistinctBy(x => x.Value).ToList();
            response.Datasheets = partInfoResults.PartResults.Datasheets.DistinctBy(x => x.Value).ToList();

            // iterate through the responses and inject PartType objects and keywords
            await InjectPartTypesAndKeywordsAsync(response, partTypes);

            var serviceResult = ServiceResult<PartResults>.Create(response);
            if (partInfoResults.ApiResponses.Any(x => x.Value.Response != null && x.Value.Response.Errors.Any()))
                serviceResult.Errors = partInfoResults.ApiResponses
                    .Where(x => x.Value.Response != null && x.Value.Response.Errors.Any())
                    .SelectMany(x => x.Value.Response!.Errors.Select(errorMessage => $"[{x.Value.Response.ApiName}] {errorMessage}"));
            return serviceResult;

            async Task InjectPartTypesAndKeywordsAsync(PartResults response, ICollection<PartType> partTypes)
            {
                foreach (var part in response.Parts)
                {
                    var partType = await DeterminePartTypeAsync(part);
                    part.PartType = partType?.Name ?? string.Empty;
                    part.PartTypeId = partType?.PartTypeId ?? 0;
                    part.Keywords = DetermineKeywordsFromPart(part, partTypes);
                }
            }

            async Task<string> DecodeBarcode(string partNumber, IUserContext? user)
            {
                var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
                if (digikeyApi.Configuration.IsConfigured)
                {
                    // 2d barcode scan requires decode first to get the partNumber being searched
                    var barcodeInfo = await GetBarcodeInfoAsync(partNumber, ScannedLabelType.Product);
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
                    if (word.Length > 1 && !ignoredWords.Contains(word, StringComparer.InvariantCultureIgnoreCase) &&
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
                if (basePart.Length > 1 && !keywords.Contains(basePart, StringComparer.InvariantCultureIgnoreCase))
                    keywords.Add(basePart);
            var mountingType = (MountingType)part.MountingTypeId;
            if (mountingType != MountingType.None && !string.IsNullOrEmpty(mountingType.ToString()) && !keywords.Contains(mountingType.ToString(), StringComparer.InvariantCultureIgnoreCase))
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

        public async Task<PartType?> DeterminePartTypeAsync(CommonPart part)
        {
            // note: partTypes call is cached
            var partTypes = await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
            var possiblePartTypes = GetMatchingPartTypes(part, partTypes);
            return possiblePartTypes
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .FirstOrDefault();
        }
    }
}
