using Binner.Model.Configuration;
using System;

namespace Binner.Common.Cache
{
    /// <summary>
    /// User configuration cache store
    /// </summary>
    public class OrganizationConfigurationCacheStore : IConfigStore
    {
        public CacheItem<OrganizationConfiguration>? OrganizationConfiguration { get; set; }
        public CacheItem<OrganizationIntegrationConfiguration>? OrganizationIntegrationConfiguration { get; set; }

        public void AddOrUpdate<TConfig>(TConfig value) where TConfig : class
        {
            if (value is OrganizationConfiguration organizationConfig)
            {
                OrganizationConfiguration = new CacheItem<OrganizationConfiguration>(organizationConfig);
            }
            else if (value is OrganizationIntegrationConfiguration integrationConfig)
            {
                OrganizationIntegrationConfiguration = new CacheItem<OrganizationIntegrationConfiguration>(integrationConfig);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported configuration type: {typeof(TConfig).Name}");
            }
        }

        public TConfig FirstOrDefault<TConfig>() where TConfig : class
        {
            if (typeof(TConfig) == typeof(OrganizationConfiguration))
            {
                var result = OrganizationConfiguration?.Value as TConfig ?? default!;
                OrganizationConfiguration?.Touch();
                return result;
            }
            else if (typeof(TConfig) == typeof(OrganizationIntegrationConfiguration))
            {
                var result = OrganizationIntegrationConfiguration?.Value as TConfig ?? default!;
                OrganizationIntegrationConfiguration?.Touch();
                return result;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported configuration type: {typeof(TConfig).Name}");
            }
        }
    }
}
