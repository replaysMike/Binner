using Binner.Model.Configuration;

namespace Binner.Model.Responses
{
    public class BarcodeSettingsResponse
    {
        /// <summary>
        /// True to enable barcode scanning features
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Set the buffer time used to filter out barcode commands. Default: 150ms (00:00:00.150)
        /// </summary>
        public TimeSpan BufferTime { get; set; } = TimeSpan.FromMilliseconds(150);

        /// <summary>
        /// Set the 2D barcode prefix, usually [)>
        /// </summary>
        public string BarcodePrefix2D { get; set; } = @"[)>";

        /// <summary>
        /// The barcode scanner profile to use
        /// </summary>
        public BarcodeProfiles Profile { get; set; } = BarcodeProfiles.Default;
    }
}
