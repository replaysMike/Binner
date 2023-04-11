using Binner.Model.Common;
using System.Collections.Generic;

namespace Binner.Common.StorageProviders
{
    public interface IStorageProviderFactory
    {
        /// <summary>
        /// Create an instance of IStorageProvider
        /// </summary>
        /// <param name="container">The service container</param>
        /// <param name="config">Configuration to pass to the storage provider</param>
        /// <returns></returns>
        IStorageProvider Create(LightInject.IServiceContainer container, IDictionary<string, string> config);
    }
}
