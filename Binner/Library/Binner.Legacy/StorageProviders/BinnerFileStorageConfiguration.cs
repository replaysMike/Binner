﻿namespace Binner.Legacy.StorageProviders
{
    public class BinnerFileStorageConfiguration
    {
        public string Filename { get; set; } = string.Empty;

        public BinnerFileStorageConfiguration() { }

        public BinnerFileStorageConfiguration(IDictionary<string, string> config)
        {
            if (config.ContainsKey("Filename"))
                Filename = config["Filename"];
        }
    }
}
