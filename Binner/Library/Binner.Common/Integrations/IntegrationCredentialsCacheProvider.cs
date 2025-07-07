using System;

namespace Binner.Common.Integrations
{
    public class IntegrationCredentialsCacheProvider : IIntegrationCredentialsCacheProvider
    {
        private static readonly Lazy<IntegrationCredentialsCache> _credentialsCache = new Lazy<IntegrationCredentialsCache>(() => new IntegrationCredentialsCache());

        /// <summary>
        /// Get the api credentials cache instance
        /// </summary>
        public IntegrationCredentialsCache Cache => _credentialsCache.Value;
    }
}
