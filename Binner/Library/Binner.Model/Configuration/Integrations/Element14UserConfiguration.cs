namespace Binner.Model.Configuration.Integrations
{
    /// <summary>
    /// Element14 Api user configuration settings
    /// </summary>
    public class Element14UserConfiguration
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Country
        /// </summary>
        public string? Country { get; set; } = "uk.farnell.com";

        /// <summary>
        /// Api key
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Path to the api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.element14.com";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiUrl);
    }
}
