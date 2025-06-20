using Binner.Common.IO;
using Binner.Global.Common;
using Binner.Model;

namespace Binner.Services
{
    public class PartScanHistoryService : IPartScanHistoryService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IRequestContextAccessor _requestContext;

        public PartScanHistoryService(IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
        }

        public async Task<PartScanHistory?> AddPartScanHistoryAsync(PartScanHistory partScanHistory)
        {
            var user = _requestContext.GetUserContext();
            if (partScanHistory.Crc == 0) partScanHistory.Crc = Checksum.Compute(partScanHistory.RawScan);
            return await _storageProvider.AddPartScanHistoryAsync(partScanHistory, user);
        }

        public async Task<bool> DeletePartScanHistoryAsync(PartScanHistory partScanHistory)
        {
            var user = _requestContext.GetUserContext();
            return await _storageProvider.DeletePartScanHistoryAsync(partScanHistory, _requestContext.GetUserContext());
        }

        public async Task<PartScanHistory?> GetPartScanHistoryAsync(string rawScan)
        {
            return await _storageProvider.GetPartScanHistoryAsync(rawScan, _requestContext.GetUserContext());
        }

        public async Task<PartScanHistory?> GetPartScanHistoryAsync(int rawScanCrc)
        {
            return await _storageProvider.GetPartScanHistoryAsync(rawScanCrc, _requestContext.GetUserContext());
        }

        public async Task<PartScanHistory?> GetPartScanHistoryAsync(long partScanHistoryId)
        {
            return await _storageProvider.GetPartScanHistoryAsync(partScanHistoryId, _requestContext.GetUserContext());
        }

        public async Task<PartScanHistory?> GetPartScanHistoryAsync(PartScanHistory partScanHistory)
        {
            return await _storageProvider.GetPartScanHistoryAsync(partScanHistory, _requestContext.GetUserContext());
        }

        public async Task<PartScanHistory?> UpdatePartScanHistoryAsync(PartScanHistory partScanHistory)
        {
            return await _storageProvider.UpdatePartScanHistoryAsync(partScanHistory, _requestContext.GetUserContext());
        }
    }
}
