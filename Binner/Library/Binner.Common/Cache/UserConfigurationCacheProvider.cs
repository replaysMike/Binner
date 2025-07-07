using Binner.Common.Integrations;
using System;

namespace Binner.Common.Cache
{
    /// <summary>
    /// Provides an in-memory cache for user configurations.
    /// </summary>
    public class UserConfigurationCacheProvider : IUserConfigurationCacheProvider
    {
        private static readonly Lazy<UserConfigurationCache> _configurationCache = new Lazy<UserConfigurationCache>(() => new UserConfigurationCache());

        /// <summary>
        /// Get the user config cache instance
        /// </summary>
        public UserConfigurationCache Cache => _configurationCache.Value;
    }
}
