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
        Task<OAuthCredential> GetOAuthCredentialAsync(string providerName);

        /// <summary>
        /// Save an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task<OAuthCredential> SaveOAuthCredentialAsync(OAuthCredential credential);

        /// <summary>
        /// Remove an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task RemoveOAuthCredentialAsync(string providerName);

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
        /// Get all parts
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        Task<ICollection<Part>> GetPartsAsync();

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
        Task<PartType> GetOrCreatePartTypeAsync(PartType partType);

        /// <summary>
        /// Create a new user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> AddProjectAsync(Project project);

        /// <summary>
        /// Get an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> GetProjectAsync(long projectId);

        /// <summary>
        /// Get an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> GetProjectAsync(string projectName);

        /// <summary>
        /// Get an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<ICollection<Project>> GetProjectsAsync();

        /// <summary>
        /// Update an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<Project> UpdateProjectAsync(Project project);

        /// <summary>
        /// Update an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<bool> DeleteProjectAsync(Project project);
    }
}
