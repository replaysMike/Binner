using System;

namespace Binner.Common.Models.Configuration.Integrations
{
    /// <summary>
    /// Stores user defined integration configurations
    /// </summary>
    public class UserIntegrationConfiguration
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int UserIntegrationConfigurationId { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Binner Swarm api enabled
        /// </summary>
        public bool SwarmEnabled { get; set; } = true;

        /// <summary>
        /// Digikey api enabled
        /// </summary>
        public bool DigiKeyEnabled { get; set; } = true;

        /// <summary>
        /// Mouser api enabled
        /// </summary>
        public bool MouserEnabled { get; set; } = true;

        /// <summary>
        /// The Api key for order management
        /// </summary>
        public string? MouserOrderApiKey { get; set; }

        /// <summary>
        /// The Api key for cart management
        /// </summary>
        public string? MouserCartApiKey { get; set; }

        /// <summary>
        /// Octopart api enabled
        /// </summary>
        public bool OctopartEnabled { get; set; } = false;

        /// <summary>
        /// Path to the Octopart Api
        /// </summary>
        public string? OctopartApiUrl { get; set; } = "https://octopart.com";

        /// <summary>
        /// Octopart Api key
        /// </summary>
        public string? OctopartApiKey { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }
    }
}
