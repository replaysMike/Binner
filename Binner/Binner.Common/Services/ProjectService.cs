using Binner.Common.Models;
using Binner.Common.StorageProviders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class ProjectService : IProjectService
    {
        private IStorageProvider _storageProvider;

        public ProjectService(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<Project> AddProjectAsync(Project project)
        {
            return await _storageProvider.AddProjectAsync(project);
        }

        public async Task<bool> DeleteProjectAsync(Project project)
        {
            return await _storageProvider.DeleteProjectAsync(project);
        }

        public async Task<Project> GetProjectAsync(long projectId)
        {
            return await _storageProvider.GetProjectAsync(projectId);
        }

        public async Task<Project> GetProjectAsync(string projectName)
        {
            return await _storageProvider.GetProjectAsync(projectName);
        }

        public async Task<ICollection<Project>> GetProjectsAsync()
        {
            return await _storageProvider.GetProjectsAsync();
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            return await _storageProvider.UpdateProjectAsync(project);
        }
    }
}
