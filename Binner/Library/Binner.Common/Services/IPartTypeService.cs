using Binner.Model.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public interface IPartTypeService
    {
        /// <summary>
        /// Add a new partType
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        Task<PartType?> AddPartTypeAsync(PartType partType);

        /// <summary>
        /// Update an existing partType
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        Task<PartType> UpdatePartTypeAsync(PartType partType);

        /// <summary>
        /// Delete an existing partType
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        Task<bool> DeletePartTypeAsync(PartType partType);

        /// <summary>
        /// Get a partType
        /// </summary>
        /// <param name="partTypeId"></param>
        /// <returns></returns>
        Task<PartType?> GetPartTypeAsync(long partTypeId);

        /// <summary>
        /// Get all part types
        /// </summary>
        /// <returns></returns>
        Task<ICollection<PartType>> GetPartTypesAsync();
    }
}
