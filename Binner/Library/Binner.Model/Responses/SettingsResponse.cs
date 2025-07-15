using Binner.Global.Common;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;

namespace Binner.Model.Responses
{
    public class SettingsResponse
    {
        /// <summary>
        /// Octopart config
        /// </summary>
        public OctopartUserConfiguration Octopart { get; set; } = new ();

        /// <summary>
        /// Digikey config
        /// </summary>
        public DigiKeyUserConfiguration Digikey { get; set; } = new ();

        /// <summary>
        /// Mouser config
        /// </summary>
        public MouserUserConfiguration Mouser { get; set; } = new ();

        /// <summary>
        /// Arrow config
        /// </summary>
        public ArrowUserConfiguration Arrow { get; set; } = new ();

        /// <summary>
        /// TME config
        /// </summary>
        public TmeUserConfiguration Tme { get; set; } = new ();

        /// <summary>
        /// Binner config
        /// </summary>
        public SwarmUserConfiguration Binner { get; set; } = new ();

        /// <summary>
        /// Printer config
        /// </summary>
        public PrinterSettingsResponse Printer { get; set; } = new ();

        /// <summary>
        /// Barcode config
        /// </summary>
        public BarcodeSettingsResponse Barcode { get; set; } = new ();

        /// <summary>
        /// Locale config
        /// </summary>
        public LocaleSettingsResponse Locale { get; set; } = new();

        /// <summary>
        /// List of defined custom fields
        /// </summary>
        public ICollection<CustomField> CustomFields { get; set; } = new List<CustomField>();

        /// <summary>
        /// KiCad integration settings
        /// </summary>
        public KiCadSettings KiCad { get; set; } = new KiCadSettings();

        /// <summary>
        /// Use the Binner module to configure the UI
        /// </summary>
        public BinnerModules UseModule { get; set; }

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

        /// <summary>
        /// Enables auto part search in the Inventory UI
        /// </summary>
        public bool EnableAutoPartSearch { get; set; } = true;

        /// <summary>
        /// Enables dark mode in the UI
        /// </summary>
        public bool EnableDarkMode { get; set; }

        /// <summary>
        /// True to enable checking for new versions of Binner
        /// </summary>
        public bool EnableCheckNewVersion { get; set; } = true;
    }
}
