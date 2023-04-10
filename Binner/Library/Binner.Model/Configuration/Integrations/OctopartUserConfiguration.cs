namespace Binner.Model.Configuration
{
    /// <summary>
    /// Octopart Api user configuration settings
    /// </summary>
    public class OctopartUserConfiguration
    {
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Client Id
        /// </summary>
        public string? ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Client Secret
        /// </summary>
        public string? ClientSecret { get; set; } = string.Empty;
    }
}
