namespace Binner.Common.Models.Configuration.Integrations
{
    public class AliExpressConfiguration : IApiConfiguration
    {
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// AliExpress Api Key
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Path to the AliExpress Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.aliexpress.com";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiUrl);
    }
}
