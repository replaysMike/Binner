namespace Binner.Model.Configuration
{
    public class MouserConfiguration : IApiConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string? ApiKey => ApiKeys.SearchApiKey;

        /// <summary>
        /// Mouser Api Keys
        /// </summary>
        public MouserApiKeys ApiKeys { get; set; } = new MouserApiKeys();

        /// <summary>
        /// Path to the Mouser Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.mouser.com";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiKeys.SearchApiKey) && !string.IsNullOrEmpty(ApiUrl);
    }

    public class MouserApiKeys
    {
        /// <summary>
        /// The Api key for search features
        /// </summary>
        public string? SearchApiKey { get; set; }

        /// <summary>
        /// The Api key for order management
        /// </summary>
        public string? OrderApiKey { get; set; }

        /// <summary>
        /// The Api key for cart management
        /// </summary>
        public string? CartApiKey { get; set; }

        /// <summary>
        /// The Api url
        /// </summary>
        public string? ApiUrl { get; set; }
    }
}
