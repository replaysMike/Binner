using Binner.Global.Common;
using Binner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class PartTypeService : IPartTypeService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IRequestContextAccessor _requestContext;

        public PartTypeService(IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
        }

        public async Task<PartType?> AddPartTypeAsync(PartType partType)
        {
            return await _storageProvider.GetOrCreatePartTypeAsync(partType, _requestContext.GetUserContext());
        }

        public async Task<bool> DeletePartTypeAsync(PartType partType)
        {
            return await _storageProvider.DeletePartTypeAsync(partType, _requestContext.GetUserContext());
        }

        public async Task<PartType?> GetPartTypeAsync(long partTypeId)
        {
            return await _storageProvider.GetPartTypeAsync(partTypeId, _requestContext.GetUserContext());
        }

        public async Task<ICollection<PartType>> GetPartTypesAsync()
        {
            return await _storageProvider.GetPartTypesAsync(_requestContext.GetUserContext());
        }

        public async Task<PartType> UpdatePartTypeAsync(PartType partType)
        {
            return await _storageProvider.UpdatePartTypeAsync(partType, _requestContext.GetUserContext());
        }
    }
}
