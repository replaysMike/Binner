using AutoMapper;
using Binner.Common.Extensions;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Binner.Services.Integrations;
using Binner.Services.Integrations.PartInformation;
using Binner.StorageProvider.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq.Expressions;
using Part = Binner.Model.Part;

namespace Binner.Services
{
    public class PartService : IPartService
    {
        protected const string MissingDatasheetCoverName = "datasheetcover.png";
        protected const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;

        protected readonly WebHostServiceConfiguration _configuration;
        protected readonly IStorageProvider _storageProvider;
        protected readonly IMapper _mapper;
        protected readonly IIntegrationApiFactory _integrationApiFactory;
        protected readonly IRequestContextAccessor _requestContext;
        protected readonly ILogger<PartService> _logger;
        protected readonly IPartTypesCache _partTypesCache;
        protected readonly IExternalOrderService _externalOrderService;
        protected readonly IExternalBarcodeInfoService _externalBarcodeInfoService;
        protected readonly IExternalPartInfoService _externalPartInfoService;
        protected readonly IExternalCategoriesService _externalCategoriesService;
        protected readonly IBaseIntegrationBehavior _baseIntegrationBehavior;

        public PartService(WebHostServiceConfiguration configuration, ILogger<PartService> logger, IStorageProvider storageProvider, IMapper mapper, IIntegrationApiFactory integrationApiFactory, IRequestContextAccessor requestContextAccessor, IPartTypesCache partTypesCache, IExternalOrderService externalOrderService, IExternalBarcodeInfoService externalBarcodeInfoService, IExternalPartInfoService externalPartInfoService, IExternalCategoriesService externalCategoriesService, IBaseIntegrationBehavior baseIntegrationBehavior)
        {
            _configuration = configuration;
            _logger = logger;
            _storageProvider = storageProvider;
            _mapper = mapper;
            _integrationApiFactory = integrationApiFactory;
            _requestContext = requestContextAccessor;
            _partTypesCache = partTypesCache;
            _externalOrderService = externalOrderService;
            _externalBarcodeInfoService = externalBarcodeInfoService;
            _externalPartInfoService = externalPartInfoService;
            _externalCategoriesService = externalCategoriesService;
            _baseIntegrationBehavior = baseIntegrationBehavior;
        }

        public virtual async Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords)
        {
            return await _storageProvider.FindPartsAsync(keywords, _requestContext.GetUserContext());
        }

        public virtual async Task<long> GetPartsCountAsync()
        {
            return await _storageProvider.GetPartsCountAsync(_requestContext.GetUserContext());
        }

        public virtual async Task<long> GetUniquePartsCountAsync()
        {
            return await _storageProvider.GetUniquePartsCountAsync(_requestContext.GetUserContext());
        }

        public virtual long GetUniquePartsMax()
        {
            return 0; // unlimited parts
        }

        public virtual async Task<decimal> GetPartsValueAsync()
        {
            return await _storageProvider.GetPartsValueAsync(_requestContext.GetUserContext());
        }

        public virtual async Task<PaginatedResponse<Part>> GetLowStockAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetLowStockAsync(request, _requestContext.GetUserContext());
        }

        public virtual async Task<Part?> GetPartAsync(GetPartRequest request)
        {
            if (request.PartId > 0)
                return await _storageProvider.GetPartAsync(request.PartId, _requestContext.GetUserContext());
            else if (!string.IsNullOrEmpty(request.ShortId))
                return await _storageProvider.GetPartByShortIdAsync(request.ShortId, _requestContext.GetUserContext());
            else
                return await _storageProvider.GetPartAsync(request.PartNumber ?? string.Empty, _requestContext.GetUserContext());
        }

        public virtual async Task<(Part? Part, ICollection<StoredFile> StoredFiles)> GetPartWithStoredFilesAsync(GetPartRequest request)
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

        public virtual async Task<PaginatedResponse<Part>> GetPartsAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetPartsAsync(request, _requestContext.GetUserContext());
        }

        public virtual async Task<ICollection<Part>> GetPartsAsync(Expression<Func<Part, bool>> condition)
        {
            return await _storageProvider.GetPartsAsync(condition, _requestContext.GetUserContext());
        }

        public virtual async Task<Part> AddPartAsync(Part part)
        {
            return await _storageProvider.AddPartAsync(part, _requestContext.GetUserContext());
        }

        public virtual async Task<Part?> UpdatePartAsync(Part part)
        {
            return await _storageProvider.UpdatePartAsync(part, _requestContext.GetUserContext());
        }

        public virtual async Task<bool> DeletePartAsync(Part part)
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

        public virtual async Task<PartType?> GetOrCreatePartTypeAsync(PartType partType)
        {
            if (partType == null) throw new ArgumentNullException(nameof(partType));
            if (partType.Name == null) throw new ArgumentNullException(nameof(partType.Name));
            return await _storageProvider.GetOrCreatePartTypeAsync(partType, _requestContext.GetUserContext());
        }

        public virtual async Task<PartType?> GetPartTypeAsync(long partTypeId)
        {
            return await _storageProvider.GetPartTypeAsync(partTypeId, _requestContext.GetUserContext());
        }

        public virtual async Task<PartType?> GetPartTypeAsync(string name)
        {
            return await _storageProvider.GetPartTypeAsync(name, _requestContext.GetUserContext());
        }

        public virtual async Task<ICollection<PartType>> GetPartTypesAsync()
        {
            return await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
        }

        public virtual async Task<ICollection<PartType>> GetPartTypesAsync(bool filterEmpty)
        {
            return await _storageProvider.GetPartTypesAsync(filterEmpty, _requestContext.GetUserContext());
        }

        public virtual Task<ICollection<PartTypeResponse>> GetPartTypesWithPartCountsAsync()
        {
            var userContext = _requestContext.GetUserContext();
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var partTypes = _partTypesCache.Cache
                .Where(x => x.OrganizationId == userContext.OrganizationId || (x.UserId == null && x.OrganizationId == null))
                .OrderBy(x => x.ParentPartType != null ? x.ParentPartType.Name : "")
                .ThenBy(x => x.Name)
                .ToList();
            return Task.FromResult(_mapper.Map<ICollection<PartTypeResponse>>(partTypes));
        }

        public virtual async Task<ICollection<Part>> GetPartsByPartTypeAsync(PartType partType)
        {
            return await _storageProvider.GetPartsByPartTypeAsync(partType, _requestContext.GetUserContext());
        }

        public virtual async Task<ICollection<PartSupplier>> GetPartSuppliersAsync(long partId)
        {
            return await _storageProvider.GetPartSuppliersAsync(partId, _requestContext.GetUserContext());
        }

        public virtual async Task<PartSupplier> AddPartSupplierAsync(PartSupplier partSupplier)
        {
            return await _storageProvider.AddPartSupplierAsync(partSupplier, _requestContext.GetUserContext());
        }

        public virtual async Task<PartSupplier?> UpdatePartSupplierAsync(PartSupplier partSupplier)
        {
            return await _storageProvider.UpdatePartSupplierAsync(partSupplier, _requestContext.GetUserContext());
        }

        public virtual async Task<bool> DeletePartSupplierAsync(PartSupplier partSupplier)
        {
            return await _storageProvider.DeletePartSupplierAsync(partSupplier, _requestContext.GetUserContext());
        }

        /// <summary>
        /// List all categories from an external provider (default: DigiKey)
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IServiceResult<CategoriesResponse?>> GetCategoriesAsync(ExternalCategoriesRequest? request = null)
        {
            return await _externalCategoriesService.GetExternalCategoriesAsync(request ?? new ExternalCategoriesRequest { Supplier = "DigiKey" });
        }

        /// <summary>
        /// Get an external order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual async Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(OrderImportRequest request)
        {
            return await _externalOrderService.GetExternalOrderAsync(request);
        }

        public virtual async Task<IServiceResult<PartResults?>> GetPartInformationAsync(string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "")
        {
            var partResults = new PartResults();

            if (string.IsNullOrEmpty(partNumber))
            {
                // return empty result, invalid request
                return ServiceResult<PartResults>.Create("No part number requested!", "Multiple");
            }

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
                        partResults.Parts.Add(new CommonPart
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

            // fetch part information
            var serviceResult = await _externalPartInfoService.GetPartInformationAsync(inventoryPart, partNumber, partType, mountingType, supplierPartNumbers);
            if (partResults.Parts.Any())
            {
                if (serviceResult.Response == null) serviceResult.Response = new PartResults();
                serviceResult.Response.Parts.InsertRange(0, partResults.Parts);
            }
            return serviceResult;
        }

        public virtual async Task<PartType?> DeterminePartTypeAsync(CommonPart part)
        {
            return await _baseIntegrationBehavior.DeterminePartTypeAsync(part);
        }

        public async Task<IServiceResult<PartResults?>> GetBarcodeInfoAsync(string barcode, ScannedLabelType barcodeType)
        {
            return await _externalBarcodeInfoService.GetBarcodeInfoAsync(barcode, barcodeType);
        }
    }
}
