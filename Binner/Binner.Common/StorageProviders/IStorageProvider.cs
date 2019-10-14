using Binner.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.StorageProviders
{
    /// <summary>
    /// A part storage provider
    /// </summary>
    public interface IStorageProvider : IDisposable
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
        /// <param name="part"></param>
        /// <returns></returns>
        Task<Part> UpdatePartAsync(Part part);

        /// <summary>
        /// Get a part by its internal id
        /// </summary>
        /// <param name="partId"></param>
        /// <returns></returns>
        Task<Part> GetPartAsync(long partId);

        /// <summary>
        /// Get a part by part number
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        Task<Part> GetPartAsync(string partNumber);

        /// <summary>
        /// Find a part that matches any keyword
        /// </summary>
        /// <param name="keywords">Space seperated keywords</param>
        /// <returns></returns>
        Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords);

        /// <summary>
        /// Delete an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<bool> DeletePartAsync(Part part);

        /// <summary>
        /// Get a partType object, or create it if it doesn't exist
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        Task<PartType> GetOrCreatePartTypeAsync(string partType);
    }
}
