using Binner.Model;
using System.Collections.Generic;

namespace Binner.Common.StorageProviders
{
    public interface IStorageProviderFactory
    {
        /// <summary>
        /// Create an instance of IStorageProvider
        /// </summary>
        /// <param name="container">The service container</param>
        /// <param name="providerName">The name of the storage provider</param>
        /// <param name="config">Configuration to pass to the storage provider</param>
        /// <returns></returns>
        IStorageProvider Create(LightInject.IServiceContainer container, string providerName, IDictionary<string, string> config);
    }
}
