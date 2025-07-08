using Binner.Common.Integrations;

namespace Binner.Common.Cache
{
    public interface IConfigurationCacheProvider<TStore> where TStore : IConfigStore
    {
        ConfigurationCache<TStore> Cache { get; }
    }
}