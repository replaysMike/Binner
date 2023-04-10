using Binner.Model.Common;
using System;
using System.Collections.Generic;

namespace Binner.Common.StorageProviders
{
    public class StorageProviderFactory : IStorageProviderFactory
    {
        public IDictionary<string, Type> Providers { get; } = new Dictionary<string, Type>();

        public StorageProviderFactory()
        {
            Providers.Add(BinnerFileStorageProvider.ProviderName.ToLower(), typeof(BinnerFileStorageProvider));
        }

        public IStorageProvider Create(string providerName, IDictionary<string, string> config)
        {
            var providerNameLowerCase = providerName.ToLower();
            if (Providers.ContainsKey(providerNameLowerCase))
            {
                var provider = Providers[providerNameLowerCase];
                var instance = Activator.CreateInstance(provider, config) as IStorageProvider ?? throw new Exception($"Unable to create StorageProvider: {providerName}");
                return instance;
            }
            else
                throw new Exception($"StorageProvider not registered: {providerName}");
        }
    }
}
