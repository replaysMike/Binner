namespace Binner.Model.Configuration.Integrations
{
    /// <summary>
    /// Octopart api configuration
    /// </summary>
    public class OctopartConfiguration : IApiConfiguration
    {
        public bool Enabled { get; set; } = false;

        public string? ApiKey => ClientId;

        /// <summary>
        /// Nexar Client Id
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Nexar Client Secret
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Path to the Octopart Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://octopart.com";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiUrl);
    }
}
