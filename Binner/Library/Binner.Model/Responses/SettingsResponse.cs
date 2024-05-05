using Binner.Model.Configuration.Integrations;

namespace Binner.Model.Responses
{
    public class SettingsResponse
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
        /// Binner config
        /// </summary>
        public SwarmUserConfiguration Binner { get; set; } = new();

        /// <summary>
        /// Printer config
        /// </summary>
        public PrinterSettingsResponse Printer { get; set; } = new();

        /// <summary>
        /// Barcode config
        /// </summary>
        public BarcodeSettingsResponse Barcode { get; set; } = new();

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
