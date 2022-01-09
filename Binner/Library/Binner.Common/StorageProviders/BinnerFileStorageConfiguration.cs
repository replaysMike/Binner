using System.Collections.Generic;

namespace Binner.Common.StorageProviders
{
    public class BinnerFileStorageConfiguration
    {
        public string Filename { get; set; }

        public BinnerFileStorageConfiguration()
        {
        }

        public BinnerFileStorageConfiguration(IDictionary<string, string> config)
        {
            if (config.ContainsKey("Filename"))
                Filename = config["Filename"];
        }
    }
}
