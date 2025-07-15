namespace Binner.Model.Configuration
{
    public class UserConfiguration
    {
        /// <summary>
        /// Default language to be used by the API.
        /// Valid values: en, br, cs, da, de, es, fi, fr, he, hu, it, ja, ko, nl, no, pl, pt, ro, sv, th, zhs, zht, bg, rm, el, hr, lt, lv, ru, sk, tr, uk
        /// </summary>
        public Languages Language { get; set; }

        /// <summary>
        /// Default currency to be used by the API.
        /// Valid currency values: USD, CAD, JPY, GBP, EUR, HKD, SGD, TWD, KRW, AUD, NZD, INR, DKK, NOK, SEK, ILS, CNY, PLN, CHF, CZK, HUF, RON, ZAR, MYR, THB, PHP
        /// </summary>
        public Currencies Currency { get; set; }

        /// <summary>
        /// True to enable searching for part information after typing
        /// </summary>
        public bool EnableAutoPartSearch { get; set; } = true;

        /// <summary>
        /// True to enable dark mode UI
        /// </summary>
        public bool EnableDarkMode { get; set; }

        /// <summary>
        /// True to enable checking for new versions of Binner
        /// </summary>
        public bool EnableCheckNewVersion { get; set; } = true;

        /// <summary>
        /// Sets the default label to use for printing part labels
        /// </summary>
        public int? DefaultPartLabelId { get; set; }

        #region Barcode

        /// <summary>
        /// True to enable barcode scanning features
        /// </summary>
        public bool BarcodeEnabled { get; set; } = true;

        /// <summary>
        /// Enabling Barcode debug mode will print diagnostic information to the browser console
        /// </summary>
        public bool BarcodeIsDebug { get; set; } = false;

        /// <summary>
        /// The maximum amount of time in milliseconds to wait between keypresses.
        /// </summary>
        public int BarcodeMaxKeystrokeThresholdMs { get; set; } = 300;

        /// <summary>
        /// Set the buffer time in milliseconds used to filter out barcode commands. Default: 80
        /// </summary>
        public int BarcodeBufferTime { get; set; } = 80;

        /// <summary>
        /// Set the 2D barcode prefix, usually [)>
        /// </summary>
        public string BarcodePrefix2D { get; set; } = @"[)>";

        /// <summary>
        /// The barcode scanner profile to use
        /// </summary>
        public BarcodeProfiles BarcodeProfile { get; set; } = BarcodeProfiles.Default;

        #endregion
    }
}
