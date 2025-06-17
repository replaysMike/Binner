using Binner.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Threading.Tasks;

namespace Binner.Services
{
    public interface IPartScanHistoryService
    {
        /// <summary>
        /// Add a new partScanHistory
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <returns></returns>
        Task<PartScanHistory?> AddPartScanHistoryAsync(PartScanHistory partScanHistory);

        /// <summary>
        /// Update an existing partScanHistory
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <returns></returns>
        Task<PartScanHistory?> UpdatePartScanHistoryAsync(PartScanHistory partScanHistory);

        /// <summary>
        /// Delete an existing partScanHistory
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <returns></returns>
        Task<bool> DeletePartScanHistoryAsync(PartScanHistory partScanHistory);

        /// <summary>
        /// Get a partScanHistory
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <returns></returns>
        Task<PartScanHistory?> GetPartScanHistoryAsync(PartScanHistory partScanHistory);

        /// <summary>
        /// Get a partScanHistory
        /// </summary>
        /// <param name="partScanHistoryId"></param>
        /// <returns></returns>
        Task<PartScanHistory?> GetPartScanHistoryAsync(long partScanHistoryId);

        /// <summary>
        /// Get a partScanHistory by its raw barcode scan
        /// </summary>
        /// <param name="rawScan"></param>
        /// <returns></returns>
        Task<PartScanHistory?> GetPartScanHistoryAsync(string rawScan);

        /// <summary>
        /// Get a partScanHistory by a 32bit crc of its raw barcode scan
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        Task<PartScanHistory?> GetPartScanHistoryAsync(int crc);
    }
}
