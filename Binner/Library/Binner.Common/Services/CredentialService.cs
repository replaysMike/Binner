using Binner.Model.Common;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class CredentialService : ICredentialService
    {
        private IStorageProvider _storageProvider;
        private RequestContextAccessor _requestContext;

        public CredentialService(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
        }

        /// <summary>
        /// Save an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<OAuthCredential> SaveOAuthCredentialAsync(OAuthCredential credential)
        {
            return await _storageProvider.SaveOAuthCredentialAsync(credential, _requestContext.GetUserContext());
        }

        /// <summary>
        /// Get a saved a oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<OAuthCredential> GetOAuthCredentialAsync(string providerName)
        {
            return await _storageProvider.GetOAuthCredentialAsync(providerName, _requestContext.GetUserContext());
        }

        /// <summary>
        /// Remove a saved a oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task RemoveOAuthCredentialAsync(string providerName)
        {
            await _storageProvider.RemoveOAuthCredentialAsync(providerName, _requestContext.GetUserContext());
        }
    }
}
