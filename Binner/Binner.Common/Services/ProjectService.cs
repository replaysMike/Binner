using Binner.Common.Models;
using Binner.Common.StorageProviders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class ProjectService : IProjectService
    {
        private IStorageProvider _storageProvider;
        private RequestContextAccessor _requestContext;

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

        public async Task<Project> GetProjectAsync(long projectId)
        {
            return await _storageProvider.GetProjectAsync(projectId, _requestContext.GetUserContext());
        }

        public async Task<Project> GetProjectAsync(string projectName)
        {
            return await _storageProvider.GetProjectAsync(projectName, _requestContext.GetUserContext());
        }

        public async Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetProjectsAsync(request, _requestContext.GetUserContext());
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            return await _storageProvider.UpdateProjectAsync(project, _requestContext.GetUserContext());
        }
    }
}
