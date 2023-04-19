namespace Binner.Model.Configuration.Integrations
{
    /// <summary>
    /// Binner Swarm Api configuration settings
    /// </summary>
    public class SwarmConfiguration : IApiConfiguration
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

        /// <summary>
        /// Api request timeout
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ApiUrl);

    }
}
