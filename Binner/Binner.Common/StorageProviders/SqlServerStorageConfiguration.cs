using System.Collections.Generic;

namespace Binner.Common.StorageProviders
{
    public class SqlServerStorageConfiguration
    {
        public string ConnectionString { get; set; }

        public SqlServerStorageConfiguration()
        {
        }

        public SqlServerStorageConfiguration(IDictionary<string, string> config)
        {
            if (config.ContainsKey("ConnectionString"))
                ConnectionString = config["ConnectionString"];
        }
    }
}
