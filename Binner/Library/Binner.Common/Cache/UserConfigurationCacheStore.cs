using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using System;

namespace Binner.Common.Cache
{
    /// <summary>
    /// User configuration cache store
    /// </summary>
    public class UserConfigurationCacheStore
    {
        public CacheItem<UserIntegrationConfiguration>? UserIntegrationConfiguration { get; set; }
        public CacheItem<UserPrinterConfiguration>? UserPrinterConfiguration { get; set; }
        public CacheItem<UserLocaleConfiguration>? UserLocaleConfiguration { get; set; }
        public CacheItem<UserBarcodeConfiguration>? UserBarcodeConfiguration { get; set; }

        public void AddOrUpdate<TConfig>(TConfig value) where TConfig : class
        {
            if (value is UserIntegrationConfiguration integrationConfig)
            {
                UserIntegrationConfiguration = new CacheItem<UserIntegrationConfiguration>(integrationConfig);
            }
            else if (value is UserPrinterConfiguration printerConfig)
            {
                UserPrinterConfiguration = new CacheItem<UserPrinterConfiguration>(printerConfig);
            }
            else if (value is UserLocaleConfiguration localeConfig)
            {
                UserLocaleConfiguration = new CacheItem<UserLocaleConfiguration>(localeConfig);
            }
            else if (value is UserBarcodeConfiguration barcodeConfig)
            {
                UserBarcodeConfiguration = new CacheItem<UserBarcodeConfiguration>(barcodeConfig);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported configuration type: {typeof(TConfig).Name}");
            }
        }

        public TConfig FirstOrDefault<TConfig>() where TConfig : class
        {
            if (typeof(TConfig) == typeof(UserIntegrationConfiguration))
            {
                var result = UserIntegrationConfiguration?.Value as TConfig ?? default!;
                UserIntegrationConfiguration?.Touch();
                return result;
            }
            else if (typeof(TConfig) == typeof(UserPrinterConfiguration))
            {
                var result = UserPrinterConfiguration?.Value as TConfig ?? default!;
                UserPrinterConfiguration?.Touch();
                return result;
            }
            else if (typeof(TConfig) == typeof(UserLocaleConfiguration))
            {
                var result = UserLocaleConfiguration?.Value as TConfig ?? default!;
                UserLocaleConfiguration?.Touch();
                return result;
            }
            else if (typeof(TConfig) == typeof(UserBarcodeConfiguration))
            {
                var result = UserBarcodeConfiguration?.Value as TConfig ?? default!;
                UserBarcodeConfiguration?.Touch();
                return result;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported configuration type: {typeof(TConfig).Name}");
            }
        }
    }
}
