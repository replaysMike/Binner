using Binner.Model;
using Binner.Model.Configuration;

namespace Binner.Common.StorageProviders
{
    public interface IStorageProviderFactory
    {
        /// <summary>
        /// Create an instance of IStorageProvider
        /// </summary>
        /// <param name="container">The service container</param>
        /// <param name="storageProviderConfiguration">The storage provider configuration section from <see cref="WebHostServiceConfiguration"/></param>
        /// <returns></returns>
        IStorageProvider Create(LightInject.IServiceContainer container, StorageProviderConfiguration storageProviderConfiguration);

        /// <summary>
        /// Create a limited instance of IStorageProvider
        /// </summary>
        /// <param name="container"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IStorageProvider CreateLimited(LightInject.IServiceContainer container, StorageProviderConfiguration configuration);
    }
}
