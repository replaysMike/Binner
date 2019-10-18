using Binner.Common.Integrations;
using Binner.Common.Models;
using Binner.Common.StorageProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class PartService : IPartService
    {
        private IStorageProvider _storageProvider;
        private OctopartApi _octopartApi;
        private DigikeyApi _digikeyApi;
        private RequestContextAccessor _requestContext;

        public PartService(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor, OctopartApi octopartApi, DigikeyApi digikeyApi)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _octopartApi = octopartApi;
            _digikeyApi = digikeyApi;
        }

        public async Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords)
        {
            return await _storageProvider.FindPartsAsync(keywords, _requestContext.GetUserContext());
        }

        public async Task<Part> GetPartAsync(string partNumber)
        {
            return await _storageProvider.GetPartAsync(partNumber, _requestContext.GetUserContext());
        }

        public async Task<ICollection<Part>> GetPartsAsync()
        {
            return await _storageProvider.GetPartsAsync(_requestContext.GetUserContext());
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
            return await _storageProvider.GetOrCreatePartTypeAsync(partType, _requestContext.GetUserContext());
        }

        public async Task<PartMetadata> GetPartMetadataAsync(string partNumber)
        {
            var dataSheets = await _octopartApi.GetDatasheetsAsync(partNumber);
            var productDetails = await _digikeyApi.GetProductInformationAsync(partNumber);
            if (productDetails.Any())
            {
                var productDetail = productDetails.First();
                return new PartMetadata
                {
                    PartNumber = partNumber,
                    DatasheetUrl = productDetail.PrimaryDatasheet,
                    AdditionalDatasheets = dataSheets ?? new List<string>(),
                    Description = productDetail.ProductDescription,
                    ManufacturerPartNumber = productDetail.ManufacturerPartNumber,
                    DetailedDescription = productDetail.DetailedDescription,
                    Cost = productDetail.UnitPrice,
                    Package = productDetail.Parameters
                        ?.Where(x => x.Parameter.Equals("Package / Case", StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => x.Value)
                        .FirstOrDefault(),
                    Manufacturer = productDetail.Manufacturer?.Value,
                    ProductStatus = productDetail.ProductStatus,
                    ProductUrl = productDetail.ProductUrl
                };
            }
            return null;
        }
    }
}
