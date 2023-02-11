namespace Binner.Common.Models.Responses
{
    public class MouserConfigurationResponse
    {
        public bool Enabled { get; set; }

        /// <summary>
        /// Path to the Mouser Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.mouser.com";

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
    }
}
