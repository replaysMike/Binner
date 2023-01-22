namespace Binner.Common.Models.Configuration.Integrations
{
    /// <summary>
    /// Octopart Api user configuration settings
    /// </summary>
    public class OctopartUserConfiguration
    {
        public bool Enabled { get; set; } = false;

        public string? ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Path to the Octopart Api
        /// </summary>
        public string? ApiUrl { get; set; } = "https://octopart.com";
    }
}
