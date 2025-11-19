using Binner.Model;
using Binner.Model.Configuration;
using System;

namespace Binner.Common.StorageProviders
{
    public interface IStorageProviderFactory
    {
        /// <summary>
        /// Create an instance of IStorageProvider
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="storageProviderConfiguration">The storage provider configuration section from <see cref="WebHostServiceConfiguration"/></param>
        /// <returns></returns>
        IStorageProvider Create(IServiceProvider serviceProvider, StorageProviderConfiguration storageProviderConfiguration);

        /// <summary>
        /// Create a limited instance of IStorageProvider
        /// </summary>
        /// <param name="serviceProvider">The service container</param>
        /// <param name="storageProviderConfiguration">The storage provider configuration section from <see cref="WebHostServiceConfiguration"/></param>
        /// <returns></returns>
        IStorageProvider CreateLimited(IServiceProvider serviceProvider, StorageProviderConfiguration storageProviderConfiguration);
    }
}
