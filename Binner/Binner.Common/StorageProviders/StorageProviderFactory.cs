using System;
using System.Collections.Generic;

namespace Binner.Common.StorageProviders
{
    public class StorageProviderFactory : IStorageProviderFactory
    {
        public static IDictionary<string, Type> Providers { get; } = new Dictionary<string, Type>();

        public StorageProviderFactory()
        {
            Providers.Add(BinnerFileStorageProvider.ProviderName, typeof(BinnerFileStorageProvider));
            Providers.Add(SqlServerStorageProvider.ProviderName, typeof(SqlServerStorageProvider));
        }

        public IStorageProvider Create(string providerName, IDictionary<string, string> config, RequestContextAccessor requestContextAccessor)
        {
            if (Providers.ContainsKey(providerName))
            {
                return Activator.CreateInstance(Providers[providerName], config, requestContextAccessor) as IStorageProvider;
            }
            else
                throw new Exception($"StorageProvider not registered: {providerName}");
        }
    }
}
