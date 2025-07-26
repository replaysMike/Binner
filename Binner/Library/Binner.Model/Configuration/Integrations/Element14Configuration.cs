namespace Binner.Model.Configuration.Integrations
{
    /// <summary>
    /// Element14 api configuration
    /// </summary>
    public class Element14Configuration : IApiConfiguration
    {
        public bool Enabled { get; set; } = false;

        public string Country { get; set; } = "uk.farnell.com";

        /// <summary>
        /// Api key (anonymous key)
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Path to the api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.element14.com";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiUrl);
    }
}
