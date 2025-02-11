using Binner.Model;
using System;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public interface ICredentialService
    {
        /// <summary>
        /// Create a formatted context key
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        string CreateContextKey(string providerName, int userId);

        /// <summary>
        /// Create an oAuth request
        /// </summary>
        /// <param name="authRequest"></param>
        /// <returns></returns>
        Task<OAuthAuthorization> CreateOAuthRequestAsync(OAuthAuthorization authRequest);

        /// <summary>
        /// Update a oAuth request
        /// </summary>
        /// <param name="authRequest"></param>
        /// <returns></returns>
        Task<OAuthAuthorization> UpdateOAuthRequestAsync(OAuthAuthorization authRequest);

        /// <summary>
        /// Get an existing (pending) oAuth request
        /// </summary>
        /// <param name="requestId">The request Id initiated the request</param>
        /// <param name="requireUserContext">True to require a valid user context, false will skip this check.</param>
        /// <returns></returns>
        Task<OAuthAuthorization?> GetOAuthRequestAsync(Guid requestId, bool requireUserContext);

        /// <summary>
        /// Save an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task<OAuthCredential> SaveOAuthCredentialAsync(OAuthCredential credential);

        /// <summary>
        /// Get a saved a oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task<OAuthCredential?> GetOAuthCredentialAsync(string providerName);

        /// <summary>
        /// Remove a saved a oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task RemoveOAuthCredentialAsync(string providerName);
    }
}
