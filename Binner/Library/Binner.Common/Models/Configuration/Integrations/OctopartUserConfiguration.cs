namespace Binner.Common.Models.Configuration.Integrations
{
    /// <summary>
    /// Octopart Api user configuration settings
    /// </summary>
    public class OctopartUserConfiguration
    {
        public bool Enabled { get; set; } = false;

        public string? ClientId { get; set; } = string.Empty;

        public string? ClientSecret { get; set; } = string.Empty;
    }
}
