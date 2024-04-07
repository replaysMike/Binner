namespace Binner.Model.Configuration.Integrations
{
    /// <summary>
    /// TME api configuration
    /// </summary>
    public class TmeConfiguration : IApiConfiguration
    {
        public bool Enabled { get; set; } = false;

        public string Country { get; set; } = "us";

        /// <summary>
        /// Api key (anonymous key)
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Application secret
        /// </summary>
        public string? ApplicationSecret { get; set; }

        /// <summary>
        /// Path to the api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.tme.eu";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiUrl) && !string.IsNullOrEmpty(ApplicationSecret);
    }
}
