using Binner.Common.Models;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public interface ICredentialService
    {
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
        Task<OAuthCredential> GetOAuthCredentialAsync(string providerName);

        /// <summary>
        /// Remove a saved a oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task RemoveOAuthCredentialAsync(string providerName);
    }
}
