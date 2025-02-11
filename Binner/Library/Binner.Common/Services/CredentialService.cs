using Binner.Global.Common;
using Binner.Model;
using System;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly RequestContextAccessor _requestContext;

        public CredentialService(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
        }

        public string CreateContextKey(string providerName, int userId)
            => $"{providerName}-{userId}";

        /// <summary>
        /// Create an (pending) oAuth request
        /// </summary>
        /// <param name="authRequest"></param>
        /// <returns></returns>
        public async Task<OAuthAuthorization> CreateOAuthRequestAsync(OAuthAuthorization authRequest)
        {

            return await _storageProvider.CreateOAuthRequestAsync(authRequest, _requestContext.GetUserContext());
        }

        /// <summary>
        /// Update a oAuth request
        /// </summary>
        /// <param name="authRequest"></param>
        /// <returns></returns>
        public async Task<OAuthAuthorization> UpdateOAuthRequestAsync(OAuthAuthorization authRequest)
        {
            return await _storageProvider.UpdateOAuthRequestAsync(authRequest, _requestContext.GetUserContext());
        }

        /// <summary>
        /// Get an existing (pending) oAuth request
        /// </summary>
        /// <param name="requestId">The request Id initiated the request</param>
        /// /// <param name="requireUserContext">True to require a valid user context, false will skip this check.</param>
        /// <returns></returns>
        public async Task<OAuthAuthorization?> GetOAuthRequestAsync(Guid requestId, bool requireUserContext)
        {
            if (requireUserContext)
            {
                var userContext = _requestContext.GetUserContext();
                if (userContext == null) throw new ArgumentNullException(nameof(userContext));

                return await _storageProvider.GetOAuthRequestAsync(requestId, userContext);
            }
            return await _storageProvider.GetOAuthRequestAsync(requestId, null);
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
        /// <param name="providerName"></param>
        /// <returns></returns>
        public async Task<OAuthCredential?> GetOAuthCredentialAsync(string providerName)
        {
            return await _storageProvider.GetOAuthCredentialAsync(providerName, _requestContext.GetUserContext());
        }

        /// <summary>
        /// Remove a saved a oAuth Credential
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public async Task RemoveOAuthCredentialAsync(string providerName)
        {
            await _storageProvider.RemoveOAuthCredentialAsync(providerName, _requestContext.GetUserContext());
        }
    }
}
