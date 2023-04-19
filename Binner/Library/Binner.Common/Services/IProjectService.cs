using Binner.Model;
using Binner.Model.Requests;
using Binner.Model.Responses;
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
        Task<ProjectPart?> AddPartAsync(AddBomPartRequest request);

        /// <summary>
        /// Update part details in project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ProjectPart?> UpdatePartAsync(UpdateBomPartRequest request);

        /// <summary>
        /// Remove part from project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> RemovePartAsync(RemoveBomPartRequest request);

        /// <summary>
        /// Get the produce history for a BOM project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ICollection<ProjectProduceHistory>> GetProduceHistoryAsync(GetProduceHistoryRequest request);

        /// <summary>
        /// Update a produce history record (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ProjectProduceHistory> UpdateProduceHistoryAsync(ProjectProduceHistory request);

        /// <summary>
        /// Delete a produce history record
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> DeleteProduceHistoryAsync(ProjectProduceHistory request);

        /// <summary>
        /// Delete a pcb produce history record
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> DeletePcbProduceHistoryAsync(ProjectPcbProduceHistory request);

        /// <summary>
        /// Produce a PCB by reducing inventory quantities within the BOM
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> ProducePcbAsync(ProduceBomPcbRequest request);
    }
}
