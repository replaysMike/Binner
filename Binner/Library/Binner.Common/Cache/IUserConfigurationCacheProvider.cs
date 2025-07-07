using Binner.Common.Integrations;

namespace Binner.Common.Cache
{
    public interface IUserConfigurationCacheProvider
    {
        UserConfigurationCache Cache { get; }
    }
}