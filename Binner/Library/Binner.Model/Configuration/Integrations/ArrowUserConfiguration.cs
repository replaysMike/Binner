namespace Binner.Model.Configuration
{
    /// <summary>
    /// Arrow api user configuration settings
    /// </summary>
    public class ArrowUserConfiguration
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Your arrow.com account username/login
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Api key
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Path to the api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.arrow.com";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiUrl);
    }
}
