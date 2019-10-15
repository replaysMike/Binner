using Binner.Common.Models;
using Binner.Common.StorageProviders;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class CredentialService :ICredentialService
    {
        private IStorageProvider _storageProvider;

        public CredentialService(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Save an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<OAuthCredential> SaveOAuthCredentialAsync(OAuthCredential credential)
        {
            return await _storageProvider.SaveOAuthCredentialAsync(credential);
        }

        /// <summary>
        /// Get a saved a oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<OAuthCredential> GetOAuthCredentialAsync(string providerName)
        {
            return await _storageProvider.GetOAuthCredentialAsync(providerName);
        }
    }
}
