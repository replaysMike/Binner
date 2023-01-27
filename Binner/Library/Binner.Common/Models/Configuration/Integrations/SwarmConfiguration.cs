namespace Binner.Common.Models.Configuration.Integrations
{
    /// <summary>
    /// Binner Swarm Api configuration settings
    /// </summary>
    public class SwarmConfiguration
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The optional api key
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Path to the Swarm Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://swarm.binner.io";
    }
}
