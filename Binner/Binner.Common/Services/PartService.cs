using Binner.Common.Api;
using Binner.Common.Models;
using Binner.Common.StorageProviders;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class PartService : IPartService
    {
        private IStorageProvider _storageProvider;
        private OctopartApi _octopartApi;

        public PartService(IStorageProvider storageProvider, OctopartApi octopartApi)
        {
            _storageProvider = storageProvider;
            _octopartApi = octopartApi;
        }

        public async Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords)
        {
            return await _storageProvider.FindPartsAsync(keywords);
        }

        public async Task<Part> GetPartAsync(string partNumber)
        {
            return await _storageProvider.GetPartAsync(partNumber);
        }

        public async Task<Part> AddPartAsync(Part part)
        {
            var dataSheets = await _octopartApi.GetDatasheetsAsync(part.PartNumber);
            part.DatasheetUrl = dataSheets.FirstOrDefault();
            return await _storageProvider.AddPartAsync(part);
        }

        public async Task<Part> UpdatePartAsync(Part part)
        {
            return await _storageProvider.UpdatePartAsync(part);
        }

        public async Task<bool> DeletePartAsync(Part part)
        {
            return await _storageProvider.DeletePartAsync(part);
        }

        public async Task<PartType> GetOrCreatePartTypeAsync(string partType)
        {
            return await _storageProvider.GetOrCreatePartTypeAsync(partType);
        }

        public async Task<PartMetadata> GetPartMetadataAsync(string partNumber)
        {
            var dataSheets = await _octopartApi.GetDatasheetsAsync(partNumber);
            return new PartMetadata
            {
                PartNumber = partNumber,
                DatasheetUrl = dataSheets.FirstOrDefault(),
                Description = "",
                Cost = 0
            };
        }
    }
}
