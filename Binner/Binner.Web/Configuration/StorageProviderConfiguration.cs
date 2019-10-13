using System.Collections.Generic;

namespace Binner.Web.Configuration
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
    }
}
