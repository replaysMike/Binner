using System;
using System.Collections.Generic;
using System.Text;

namespace Binner.Web.Configuration
{
    public class StorageProviderConfiguration
    {
        public StorageProvider Provider { get; set; }
        public IDictionary<string, string> ProviderConfiguration { get; set; } = new Dictionary<string, string>();
    }

    public enum StorageProvider
    {
        Binner
    }
}
