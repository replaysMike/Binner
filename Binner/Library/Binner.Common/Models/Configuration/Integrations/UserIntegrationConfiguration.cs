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
        /// Swarm api key
        /// </summary>
        public string? SwarmApiKey { get; set; }

        /// <summary>
        /// Swarm api url
        /// </summary>
        public string? SwarmApiUrl { get; set; }

        /// <summary>
        /// Swarm api request timeout
        /// </summary>
        public TimeSpan? SwarmTimeout { get; set; }

        /// <summary>
        /// DigiKey api enabled
        /// </summary>
        public bool DigiKeyEnabled { get; set; } = true;

        /// <summary>
        /// DigiKey client Id
        /// </summary>
        public string? DigiKeyClientId { get; set; }


        /// <summary>
        /// DigiKey client secret
        /// </summary>
        public string? DigiKeyClientSecret { get; set; }

        /// <summary>
        /// Path to oAuth postback Url
        /// </summary>
        public string DigiKeyOAuthPostbackUrl { get; set; } = "https://localhost:8090/Authorization/Authorize";

        /// <summary>
        /// DigiKey api url
        /// </summary>
        public string DigiKeyApiUrl { get; set; } = "https://sandbox-api.digikey.com";

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
        /// The Api key for part search
        /// </summary>
        public string? MouserSearchApiKey { get; set; }

        /// <summary>
        /// Mouser api url
        /// </summary>
        public string MouserApiUrl { get; set; } = "https://api.mouser.com";

        /// <summary>
        /// Octopart api enabled
        /// </summary>
        public bool OctopartEnabled { get; set; } = false;

        /// <summary>
        /// Path to the Octopart Api
        /// </summary>
        public string OctopartApiUrl { get; set; } = "https://octopart.com";

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
