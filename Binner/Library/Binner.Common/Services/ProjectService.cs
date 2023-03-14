using Binner.Common.Models.Requests;
using Binner.Model.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly RequestContextAccessor _requestContext;

        public ProjectService(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
        }

        public async Task<Project> AddProjectAsync(Project project)
        {
            return await _storageProvider.AddProjectAsync(project, _requestContext.GetUserContext());
        }

        public async Task<bool> DeleteProjectAsync(Project project)
        {
            return await _storageProvider.DeleteProjectAsync(project, _requestContext.GetUserContext());
        }

        public async Task<Project?> GetProjectAsync(long projectId)
        {
            return await _storageProvider.GetProjectAsync(projectId, _requestContext.GetUserContext());
        }

        public async Task<Project?> GetProjectAsync(string name)
        {
            return await _storageProvider.GetProjectAsync(name, _requestContext.GetUserContext());
        }

        public async Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetProjectsAsync(request, _requestContext.GetUserContext());
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            return await _storageProvider.UpdateProjectAsync(project, _requestContext.GetUserContext());
        }

        /// <summary>
        /// Add a part to the project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Project?> AddPartAsync(AddBomPartRequest request)
        {
            Project? project = null;
            
            if (request.ProjectId != null)
                project = await _storageProvider.GetProjectAsync(request.ProjectId.Value, _requestContext.GetUserContext());
            else if (!string.IsNullOrEmpty(request.Project))
                project = await _storageProvider.GetProjectAsync(request.Project, _requestContext.GetUserContext());
            
            if (project == null)
                return null;

            var part = await _storageProvider.GetPartAsync(request.PartNumber, _requestContext.GetUserContext());
            if (part != null)
            {
                var assignment = new ProjectPartAssignment
                {
                    PartId = part.PartId,
                    ProjectId = project.ProjectId,
                    Notes = "",
                    PartName = null,
                    PcbId = null,
                    Quantity = 1,
                    ReferenceId = null,
                };
                await _storageProvider.AddProjectPartAssignmentAsync(assignment, _requestContext.GetUserContext());
                // return the full project
                return await GetProjectAsync(project.ProjectId);
            }

            return null;
        }

        /// <summary>
        /// Update part details in project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Project?> UpdatePartAsync(UpdateBomPartRequest request)
        {
            Project? project = null;
            
            if (request.ProjectId != null)
                project = await _storageProvider.GetProjectAsync(request.ProjectId.Value, _requestContext.GetUserContext());
            else if (!string.IsNullOrEmpty(request.Project))
                project = await _storageProvider.GetProjectAsync(request.Project, _requestContext.GetUserContext());
            
            if (project == null)
                return null;

            var part = await _storageProvider.GetPartAsync(request.PartNumber, _requestContext.GetUserContext());
            if (part != null)
            {
                var assignment = await _storageProvider.GetProjectPartAssignmentAsync(assignment, _requestContext.GetUserContext());
                if (assignment != null)
                {
                    await _storageProvider.UpdateProjectPartAssignmentAsync(assignment,
                        _requestContext.GetUserContext());
                    // return the full project
                    return await GetProjectAsync(project.ProjectId);
                }
            }

            return null;
        }

        /// <summary>
        /// Remove part from project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> RemovePartAsync(RemoveBomPartRequest request)
        {
            Project? project = null;
            
            if (request.ProjectId != null)
                project = await _storageProvider.GetProjectAsync(request.ProjectId.Value, _requestContext.GetUserContext());
            else if (!string.IsNullOrEmpty(request.Project))
                project = await _storageProvider.GetProjectAsync(request.Project, _requestContext.GetUserContext());
            
            if (project == null)
                return false;

            var part = await _storageProvider.GetPartAsync(request.PartNumber, _requestContext.GetUserContext());
            if (part != null)
                return await _storageProvider.RemoveProjectPartAsync(project, part, _requestContext.GetUserContext());
            return false;
        }
    }
}
