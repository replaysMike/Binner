namespace Binner.Common.Models.Configuration.Integrations
{
    public class MouserConfiguration
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Mouser Api Keys
        /// </summary>
        public MouserApiKeys ApiKeys { get; set; } = new MouserApiKeys();

        /// <summary>
        /// Path to the Mouser Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.mouser.com";
    }

    public class MouserApiKeys
    {
        /// <summary>
        /// The Api key for search features
        /// </summary>
        public string SearchApiKey { get; set; }

        /// <summary>
        /// The Api key for order management
        /// </summary>
        public string OrderApiKey { get; set; }

        /// <summary>
        /// The Api key for cart management
        /// </summary>
        public string CartApiKey { get; set; }
    }
}
