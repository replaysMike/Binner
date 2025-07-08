using Binner.Common.Integrations;
using System;

namespace Binner.Common.Cache
{
    /// <summary>
    /// Provides an in-memory cache for organization configurations.
    /// </summary>
    public class OrganizationConfigurationCacheProvider : IOrganizationConfigurationCacheProvider
    {
        private static readonly Lazy<ConfigurationCache<OrganizationConfigurationCacheStore>> _configurationCache = new Lazy<ConfigurationCache<OrganizationConfigurationCacheStore>>(() => new ConfigurationCache<OrganizationConfigurationCacheStore>());

        /// <summary>
        /// Get the user config cache instance
        /// </summary>
        public ConfigurationCache<OrganizationConfigurationCacheStore> Cache => _configurationCache.Value;
    }
}
