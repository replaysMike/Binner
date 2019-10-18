using System;
using System.Collections.Generic;

namespace Binner.Common.StorageProviders
{
    public interface IStorageProviderFactory
    {
        /// <summary>
        /// Create an instance of IStorageProvider
        /// </summary>
        /// <param name="providerName">The registered provider name</param>
        /// <param name="config">Configuration to pass to the storage provider</param>
        /// <param name="requestContextAccessor">Request context accessor</param>
        /// <returns></returns>
        IStorageProvider Create(string providerName, IDictionary<string, string> config, RequestContextAccessor requestContextAccessor);
    }
}
