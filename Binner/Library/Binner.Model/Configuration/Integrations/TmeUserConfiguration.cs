using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Model.Configuration.Integrations
{
    /// <summary>
    /// TME Api user configuration settings
    /// </summary>
    public class TmeUserConfiguration
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Country
        /// </summary>
        public string? Country { get; set; } = "us";

        /// <summary>
        /// Api key
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Your TME application secret
        /// </summary>
        public string? ApplicationSecret { get; set; }

        /// <summary>
        /// Path to the api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.tme.eu";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiUrl) && !string.IsNullOrEmpty(ApplicationSecret);
    }
}
