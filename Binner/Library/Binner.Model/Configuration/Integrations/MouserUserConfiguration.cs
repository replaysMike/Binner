namespace Binner.Model.Configuration.Integrations
{
    /// <summary>
    /// Mouser Api user configuration settings
    /// </summary>
    public class MouserUserConfiguration
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The Api key for order management
        /// </summary>
        public string? OrderApiKey { get; set; } = string.Empty;

        /// <summary>
        /// The Api key for cart management
        /// </summary>
        public string? CartApiKey { get; set; } = string.Empty;

        /// <summary>
        /// The Api key for search features
        /// </summary>
        public string? SearchApiKey { get; set; }

        /// <summary>
        /// Mouser api url
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.mouser.com";
    }
}
