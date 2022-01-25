namespace Binner.Common.Models.Configuration
{
    public class IntegrationConfiguration
    {
        /// <summary>
        /// Octopart config
        /// </summary>
        public OctopartConfiguration Octopart { get; set; } = new OctopartConfiguration();

        /// <summary>
        /// Digikey config
        /// </summary>
        public DigikeyConfiguration Digikey { get; set; } = new DigikeyConfiguration();

        /// <summary>
        /// Mouser config
        /// </summary>
        public MouserConfiguration Mouser { get; set; } = new MouserConfiguration();

        /// <summary>
        /// AliExpress config
        /// </summary>
        public AliExpressConfiguration AliExpress { get; set; } = new AliExpressConfiguration();

    }
}
