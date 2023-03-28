namespace Binner.Common.Models.Configuration.Integrations
{
    public class IntegrationConfiguration
    {
        /// <summary>
        /// Binner Swarm config
        /// </summary>
        public SwarmConfiguration Swarm { get; set; } = new SwarmConfiguration();

        /// <summary>
        /// Octopart config
        /// </summary>
        public OctopartConfiguration Nexar { get; set; } = new OctopartConfiguration();

        /// <summary>
        /// Digikey config
        /// </summary>
        public DigikeyConfiguration Digikey { get; set; } = new DigikeyConfiguration();

        /// <summary>
        /// Mouser config
        /// </summary>
        public MouserConfiguration Mouser { get; set; } = new MouserConfiguration();

        /// <summary>
        /// Arrow config
        /// </summary>
        public ArrowConfiguration Arrow { get; set; } = new ArrowConfiguration();

        /// <summary>
        /// AliExpress config
        /// </summary>
        public AliExpressConfiguration AliExpress { get; set; } = new AliExpressConfiguration();

    }
}
