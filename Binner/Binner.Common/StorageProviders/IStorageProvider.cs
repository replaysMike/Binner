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
        /// Get an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task<OAuthCredential> GetOAuthCredentialAsync(string providerName, IUserContext userContext);

        /// <summary>
        /// Save an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task<OAuthCredential> SaveOAuthCredentialAsync(OAuthCredential credential, IUserContext userContext);

        /// <summary>
        /// Remove an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task RemoveOAuthCredentialAsync(string providerName, IUserContext userContext);

        /// <summary>
        /// Add a new part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<Part> AddPartAsync(Part part, IUserContext userContext);

        /// <summary>
        /// Update an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<Part> UpdatePartAsync(Part part, IUserContext userContext);

        /// <summary>
        /// Get a part by its internal id
        /// </summary>
        /// <param name="partId"></param>
        /// <returns></returns>
        Task<Part> GetPartAsync(long partId, IUserContext userContext);

        /// <summary>
        /// Get a part by part number
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        Task<Part> GetPartAsync(string partNumber, IUserContext userContext);

        /// <summary>
        /// Get all parts
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        Task<ICollection<Part>> GetPartsAsync(PaginatedRequest request, IUserContext userContext);

        /// <summary>
        /// Find a part that matches any keyword
        /// </summary>
        /// <param name="keywords">Space seperated keywords</param>
        /// <returns></returns>
        Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords, IUserContext userContext);

        /// <summary>
        /// Delete an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<bool> DeletePartAsync(Part part, IUserContext userContext);

        /// <summary>
        /// Get a partType object, or create it if it doesn't exist
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        Task<PartType> GetOrCreatePartTypeAsync(PartType partType, IUserContext userContext);

        /// <summary>
        /// Get all of the part types
        /// </summary>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ICollection<PartType>> GetPartTypesAsync(IUserContext userContext);

        /// <summary>
        /// Create a new user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> AddProjectAsync(Project project, IUserContext userContext);

        /// <summary>
        /// Get an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> GetProjectAsync(long projectId, IUserContext userContext);

        /// <summary>
        /// Get an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> GetProjectAsync(string projectName, IUserContext userContext);

        /// <summary>
        /// Get an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request, IUserContext userContext);

        /// <summary>
        /// Update an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> UpdateProjectAsync(Project project, IUserContext userContext);

        /// <summary>
        /// Update an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<bool> DeleteProjectAsync(Project project, IUserContext userContext);
    }
}
