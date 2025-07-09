using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Responses;

namespace Binner.Services
{
    public interface IOrganizationService<TOrg>
        where TOrg : class, IOrganization
    {
        /// <summary>
        /// Create a new organization
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        Task<TOrg> CreateOrganizationAsync(TOrg organization);

        /// <summary>
        /// Delete an existing organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<bool> DeleteOrganizationAsync(int organizationId);

        /// <summary>
        /// Get a organization
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        Task<TOrg?> GetOrganizationAsync(TOrg organization);

        /// <summary>
        /// Get a list of organizations
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PaginatedResponse<TOrg>> GetOrganizationsAsync(PaginatedRequest request);

        /// <summary>
        /// Update existing organization
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        Task<TOrg?> UpdateOrganizationAsync(TOrg organization);
    }
}