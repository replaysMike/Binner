using Binner.Model.Common;
using Binner.StorageProvider.MySql;
using Binner.StorageProvider.Postgresql;
using Binner.StorageProvider.Sqlite;
using Binner.StorageProvider.SqlServer;
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
            Providers.Add(MySqlStorageProvider.ProviderName.ToLower(), typeof(MySqlStorageProvider));
            Providers.Add(PostgresqlStorageProvider.ProviderName.ToLower(), typeof(PostgresqlStorageProvider));
            Providers.Add(SqliteStorageProvider.ProviderName.ToLower(), typeof(SqliteStorageProvider));
            Providers.Add(SqlServerStorageProvider.ProviderName.ToLower(), typeof(SqlServerStorageProvider));
        }

        public IStorageProvider Create(string providerName, IDictionary<string, string> config)
        {
            var providerNameLowerCase = providerName.ToLower();
            if (Providers.ContainsKey(providerNameLowerCase))
            {
                var provider = Providers[providerNameLowerCase];
                return Activator.CreateInstance(provider, config) as IStorageProvider;
            }
            else
                throw new Exception($"StorageProvider not registered: {providerName}");
        }
    }
}
