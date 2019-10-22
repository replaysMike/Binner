using Binner.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public interface IPartService
    {
        /// <summary>
        /// Add a new part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<Part> AddPartAsync(Part part);

        /// <summary>
        /// Update an existing part
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        Task<Part> UpdatePartAsync(Part part);

        /// <summary>
        /// Delete an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<bool> DeletePartAsync(Part part);

        /// <summary>
        /// Find parts
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords);

        /// <summary>
        /// Get a part by part number
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        Task<Part> GetPartAsync(string partNumber);

        /// <summary>
        /// Get all parts
        /// </summary>
        /// <returns></returns>
        Task<ICollection<Part>> GetPartsAsync(PaginatedRequest request);

        /// <summary>
        /// Get a partType object, or create it if it doesn't exist
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        Task<PartType> GetOrCreatePartTypeAsync(PartType partType);

        /// <summary>
        /// Get metadata about a part number
        /// </summary>
        /// <param name="partNumber">Part number</param>
        /// <param name="partType">Part type</param>
        /// <param name="packageType">Package type</param>
        /// <returns></returns>
        Task<PartResults> GetPartInformationAsync(string partNumber, string partType = "", string packageType = "");

        /// <summary>
        /// Get metadata about a part number
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        Task<PartMetadata> GetPartMetadataAsync(string partNumber);

        /// <summary>
        /// Get all part types
        /// </summary>
        /// <returns></returns>
        Task<ICollection<PartType>> GetPartTypesAsync();
    }
}