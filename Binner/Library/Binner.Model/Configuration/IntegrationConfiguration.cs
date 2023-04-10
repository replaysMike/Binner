﻿namespace Binner.Model.Configuration
{
    public class IntegrationConfiguration
    {
        /// <summary>
        /// Binner Swarm config
        /// </summary>
        public SwarmConfiguration Swarm { get; set; } = new ();

        /// <summary>
        /// Octopart config
        /// </summary>
        public OctopartConfiguration Octopart { get; set; } = new ();

        /// <summary>
        /// Digikey config
        /// </summary>
        public DigikeyConfiguration Digikey { get; set; } = new ();

        /// <summary>
        /// Mouser config
        /// </summary>
        public MouserConfiguration Mouser { get; set; } = new ();

        /// <summary>
        /// Arrow config
        /// </summary>
        public ArrowConfiguration Arrow { get; set; } = new ();

        /// <summary>
        /// AliExpress config
        /// </summary>
        public AliExpressConfiguration AliExpress { get; set; } = new ();

    }
}
