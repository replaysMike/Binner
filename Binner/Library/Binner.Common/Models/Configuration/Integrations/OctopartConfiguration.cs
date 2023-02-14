namespace Binner.Common.Models.Configuration.Integrations
{
    public class OctopartConfiguration : IApiConfiguration
    {
        public bool Enabled { get; set; } = false;

        public string? ApiKey { get; set; }

        /// <summary>
        /// Path to the Octopart Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://octopart.com";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiUrl);
    }
}
