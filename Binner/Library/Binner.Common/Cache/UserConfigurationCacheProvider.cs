using Binner.Common.Integrations;
using System;

namespace Binner.Common.Cache
{
    /// <summary>
    /// Provides an in-memory cache for user configurations.
    /// </summary>
    public class UserConfigurationCacheProvider : IUserConfigurationCacheProvider
    {
        private static readonly Lazy<ConfigurationCache<UserConfigurationCacheStore>> _configurationCache = new Lazy<ConfigurationCache<UserConfigurationCacheStore>>(() => new ConfigurationCache<UserConfigurationCacheStore>());

        /// <summary>
        /// Get the user config cache instance
        /// </summary>
        public ConfigurationCache<UserConfigurationCacheStore> Cache => _configurationCache.Value;
    }
}
