using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Responses;

namespace Binner.Model.Requests
{
    public class SettingsRequest
    {
        /// <summary>
        /// Octopart config
        /// </summary>
        public OctopartUserConfiguration Octopart { get; set; } = new();

        /// <summary>
        /// Digikey config
        /// </summary>
        public DigiKeyUserConfiguration Digikey { get; set; } = new();

        /// <summary>
        /// Mouser config
        /// </summary>
        public MouserUserConfiguration Mouser { get; set; } = new();

        /// <summary>
        /// Arrow config
        /// </summary>
        public ArrowUserConfiguration Arrow { get; set; } = new();

        /// <summary>
        /// TME config
        /// </summary>
        public TmeUserConfiguration Tme { get; set; } = new();

        /// <summary>
        /// Binner swarm config
        /// </summary>
        public SwarmUserConfiguration Binner { get; set; } = new();

        /// <summary>
        /// Printer configuration
        /// </summary>
        public PrinterSettingsResponse Printer { get; set; } = new();

        /// <summary>
        /// Barcode configuration
        /// </summary>
        public BarcodeConfiguration Barcode { get; set; } = new();

        /// <summary>
        /// Default language
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Default currency
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// License key
        /// </summary>
        public string LicenseKey { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of items that can be cached
        /// </summary>
        public int MaxCacheItems { get; set; } = 1024;

        /// <summary>
        /// Sliding cache expiration in minutes
        /// </summary>
        public int CacheSlidingExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Absolute cache expiration in minutes (0 = never)
        /// </summary>
        public int CacheAbsoluteExpirationMinutes { get; set; } = 0;
    }
}
