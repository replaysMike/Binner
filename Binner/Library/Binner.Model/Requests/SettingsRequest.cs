using Binner.Global.Common;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.Model.Responses;

namespace Binner.Model.Requests
{
    public class SettingsRequest
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
        /// Element14 config
        /// </summary>
        public Element14UserConfiguration Element14 { get; set; } = new();

        /// <summary>
        /// Binner swarm config
        /// </summary>
        public SwarmUserConfiguration Binner { get; set; } = new ();

        /// <summary>
        /// List of user designed custom fields
        /// </summary>
        public ICollection<CustomField> CustomFields { get; set; } = new List<CustomField>();

        /// <summary>
        /// KiCad integration settings
        /// </summary>
        public KiCadSettings KiCad { get; set; } = new KiCadSettings();

        /// <summary>
        /// Printer configuration
        /// </summary>
        public PrinterSettingsResponse Printer { get; set; } = new ();

        /// <summary>
        /// Barcode configuration
        /// </summary>
        public BarcodeConfiguration Barcode { get; set; } = new();

        /// <summary>
        /// Locale configuration
        /// </summary>
        public LocaleConfiguration Locale { get; set; } = new();

        /// <summary>
        /// License key
        /// </summary>
        public string LicenseKey { get; set; } = string.Empty;

        /// <summary>
        /// True to enable searching for part information after typing
        /// </summary>
        public bool EnableAutoPartSearch { get; set; }

        /// <summary>
        /// True to enable dark mode UI
        /// </summary>
        public bool EnableDarkMode { get; set; }

        /// <summary>
        /// True to enable checking for new versions of Binner
        /// </summary>
        public bool EnableCheckNewVersion { get; set; }

        /// <summary>
        /// Maximum number of items that can be cached
        /// </summary>
        public int MaxCacheItems { get; set; } = 1024;

        /// <summary>
        /// Use the Binner module to configure the UI
        /// </summary>
        public BinnerModules UseModule { get; set; }

        /// <summary>
        /// Sliding cache expiration in minutes
        /// </summary>
        public int CacheSlidingExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Absolute cache expiration in minutes (0 = never)
        /// </summary>
        public int CacheAbsoluteExpirationMinutes { get; set; } = 0;

        /// <summary>
        /// If true, allows fetching part metadata for parts that already exist in inventory
        /// </summary>
        public bool enableAutomaticMetadataFetchingForExistingParts { get; set; } = true;
    }
}
