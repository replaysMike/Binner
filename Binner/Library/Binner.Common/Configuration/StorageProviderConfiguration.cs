using System.Collections.Generic;

namespace Binner.Common.Configuration
{
    public class StorageProviderConfiguration
    {
        /// <summary>
        /// The storage provider to use
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Configuration to pass to the provider
        /// </summary>
        public IDictionary<string, string> ProviderConfiguration { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The path to store user uploaded files to
        /// </summary>
        public string UserUploadedFilesPath { get;set;}
    }
}
