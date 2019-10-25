using Binner.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        /// Get parts based on a condition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        Task<ICollection<Part>> GetPartsAsync(Expression<Func<Part, bool>> condition);

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
        /// <param name="mountingType">Mounting type</param>
        /// <returns></returns>
        Task<IServiceResult<PartResults>> GetPartInformationAsync(string partNumber, string partType = "", string mountingType = "");

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

        /// <summary>
        /// Get count of all parts
        /// </summary>
        /// <returns></returns>
        Task<long> GetPartsCountAsync();

        /// <summary>
        /// Get count of all parts
        /// </summary>
        /// <returns></returns>
        Task<ICollection<Part>> GetLowStockAsync();
    }
}