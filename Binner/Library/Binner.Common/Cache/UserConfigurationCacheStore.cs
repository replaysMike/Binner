using Binner.Model.Configuration;
using System;

namespace Binner.Common.Cache
{
    /// <summary>
    /// User configuration cache store
    /// </summary>
    public class UserConfigurationCacheStore : IConfigStore
    {
        public CacheItem<UserConfiguration>? UserConfiguration { get; set; }
        public CacheItem<UserPrinterConfiguration>? UserPrinterConfiguration { get; set; }

        public void AddOrUpdate<TConfig>(TConfig value) where TConfig : class
        {
            if (value is UserConfiguration userConfig)
            {
                UserConfiguration = new CacheItem<UserConfiguration>(userConfig);
            }
            else if (value is UserPrinterConfiguration printerConfig)
            {
                UserPrinterConfiguration = new CacheItem<UserPrinterConfiguration>(printerConfig);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported configuration type: {typeof(TConfig).Name}");
            }
        }

        public TConfig FirstOrDefault<TConfig>() where TConfig : class
        {
            if (typeof(TConfig) == typeof(UserConfiguration))
            {
                var result = UserConfiguration?.Value as TConfig ?? default!;
                UserConfiguration?.Touch();
                return result;
            }
            else if (typeof(TConfig) == typeof(UserPrinterConfiguration))
            {
                var result = UserPrinterConfiguration?.Value as TConfig ?? default!;
                UserPrinterConfiguration?.Touch();
                return result;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported configuration type: {typeof(TConfig).Name}");
            }
        }
    }
}
