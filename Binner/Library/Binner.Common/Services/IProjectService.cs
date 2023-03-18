using Binner.Common.Models;
using Binner.Common.Models.Requests;
using Binner.Model.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public interface IProjectService
    {
        /// <summary>
        /// Add a new project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> AddProjectAsync(Project project);

        /// <summary>
        /// Update an existing project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> UpdateProjectAsync(Project project);

        /// <summary>
        /// Delete an existing project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<bool> DeleteProjectAsync(Project project);

        /// <summary>
        /// Get a project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<Project?> GetProjectAsync(long projectId);

        /// <summary>
        /// Get a project
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<Project?> GetProjectAsync(string name);

        /// <summary>
        /// Get all projects
        /// </summary>
        /// <returns></returns>
        Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request);

        
        /// <summary>
        /// Get parts for a project (BOM)
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<ICollection<ProjectPart>> GetPartsAsync(long projectId);

        /// <summary>
        /// Get pcbs for a project (BOM)
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<ICollection<ProjectPcb>> GetPcbsAsync(long projectId);

        /// <summary>
        /// Add a part to the project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Project?> AddPartAsync(AddBomPartRequest request);

        /// <summary>
        /// Update part details in project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Project?> UpdatePartAsync(UpdateBomPartRequest request);

        /// <summary>
        /// Remove part from project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> RemovePartAsync(RemoveBomPartRequest request);

        /// <summary>
        /// Produce a PCB by reducing inventory quantities within the BOM
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> ProducePcbAsync(ProduceBomPcbRequest request);
    }
}
