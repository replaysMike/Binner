using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    /// <summary>
    /// An in-memory cache of api integration credentials per user
    /// </summary>
    public class IntegrationCredentialsCache
    {
        private readonly Dictionary<ApiCredentialKey, List<ApiCredential>> _credentials = new Dictionary<ApiCredentialKey, List<ApiCredential>>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Returns true if the cache contains a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(ApiCredentialKey key)
        {
            _lock.Wait();
            try
            {
                return _credentials.ContainsKey(key);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Clear all credentials for a given key
        /// </summary>
        /// <param name="key"></param>
        public void Clear(ApiCredentialKey key)
        {
            _lock.Wait();
            try
            {
                if (_credentials.ContainsKey(key))
                {
                    _credentials.Remove(key);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Get or add a new set of api credential for a given user
        /// </summary>
        /// <param name="key"></param>
        /// <param name="apiName"></param>
        /// <param name="addMethod"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DataException"></exception>
        public async Task<ApiCredential> GetOrAddCredentialAsync(ApiCredentialKey key, string apiName, Func<Task<ApiCredentialConfiguration>> addMethod)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(apiName)) throw new ArgumentNullException(nameof(apiName));
            if (addMethod == null) throw new ArgumentNullException(nameof(addMethod));

            await _lock.WaitAsync();
            try
            {
                ApiCredential? credential = null;
                if (_credentials.ContainsKey(key))
                {
                    var credentials = _credentials[key];
                    credential = credentials.FirstOrDefault(x => x.ApiName?.Equals(apiName) == true);
                    if (credential == null)
                    {
                        // wipe out the whole configuration and reload - apiName should not be missing!
                        _credentials.Remove(key);
                    }
                    else
                    {
                        // return the credential requested
                        return credential;
                    }
                }

                // a new credentials set for the user must be fetched
                var configuration = await CreateNewCredentialConfigurationAsync(addMethod);
                _credentials.Add(key, configuration.ApiCredentials);
                if (_credentials.ContainsKey(key))
                {
                    // return the credential requested
                    var credentials = _credentials[key];
                    credential = credentials.FirstOrDefault(x => x.ApiName == apiName);
                    if (credential != null)
                        return credential;
                }

                // fatal exception - api credentials were not properly created
                throw new DataException($"Fatal error! No api credentials were created of type '{apiName}' for user '{key.UserId}'");
            }
            finally
            {
                _lock.Release();
            }

            static async Task<ApiCredentialConfiguration> CreateNewCredentialConfigurationAsync(Func<Task<ApiCredentialConfiguration>> addMethod)
            {
                ApiCredentialConfiguration credentialConfiguration = await addMethod();
                if (credentialConfiguration == null)
                    throw new DataException($"Tried to ask for new Api credential, addMethod invokation returned null!");

                return credentialConfiguration;
            }
        }
    }
}
