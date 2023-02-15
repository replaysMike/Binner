namespace Binner.Common.Models.Configuration.Integrations
{
    /// <summary>
    /// Binner Swarm Api user configuration settings
    /// </summary>
    public class SwarmUserConfiguration
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The optional api key
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Path to the Swarm Api
        /// </summary>
        public string? ApiUrl { get; set; } = "https://swarm.binner.io";
    }
}
