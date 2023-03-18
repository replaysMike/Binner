using Binner.Common.Models.Requests;
using Binner.Model.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public interface IPcbService
    {
        /// <summary>
        /// Add a new pcb
        /// </summary>
        /// <param name="pcb"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<Pcb> AddPcbAsync(Pcb pcb, long projectId);

        /// <summary>
        /// Update an existing pcb
        /// </summary>
        /// <param name="pcb"></param>
        /// <returns></returns>
        Task<Pcb?> UpdatePcbAsync(Pcb pcb);

        /// <summary>
        /// Delete an existing pcb
        /// </summary>
        /// <param name="pcb"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<bool> DeletePcbAsync(Pcb pcb, long projectId);

        /// <summary>
        /// Get a pcb
        /// </summary>
        /// <param name="pcbId"></param>
        /// <returns></returns>
        Task<Pcb?> GetPcbAsync(long pcbId);

        /// <summary>
        /// Get a list of pcbs for a project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<ICollection<Pcb>> GetPcbsAsync(long projectId);
    }
}
