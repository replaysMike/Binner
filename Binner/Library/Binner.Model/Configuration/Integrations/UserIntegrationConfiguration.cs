namespace Binner.Model.Configuration.Integrations
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
        public DigikeyLocaleSite DigiKeySite { get; set; } = DigikeyLocaleSite.US;

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
        /// Arrow api enabled
        /// </summary>
        public bool ArrowEnabled { get; set; } = true;

        /// <summary>
        /// The user's Arrow username
        /// </summary>
        public string? ArrowUsername { get; set; }

        /// <summary>
        /// The Api key
        /// </summary>
        public string? ArrowApiKey { get; set; }

        /// <summary>
        /// Arrow api url
        /// </summary>
        public string ArrowApiUrl { get; set; } = "https://api.arrow.com";

        /// <summary>
        /// Nexar api enabled
        /// </summary>
        public bool NexarEnabled { get; set; } = false;

        /// <summary>
        /// Nexar Client Id
        /// </summary>
        public string? NexarClientId { get; set; }

        /// <summary>
        /// Nexar Client Secret
        /// </summary>
        public string? NexarClientSecret { get; set; }

        /// <summary>
        /// TME api enabled
        /// </summary>
        public bool TmeEnabled { get; set; } = true;

        /// <summary>
        /// TME api Country
        /// </summary>
        public string TmeCountry { get; set; } = "us";

        /// <summary>
        /// The user's TME application secret
        /// </summary>
        public string? TmeApplicationSecret { get; set; }

        /// <summary>
        /// The Api key
        /// </summary>
        public string? TmeApiKey { get; set; }

        /// <summary>
        /// TME api url
        /// </summary>
        public string TmeApiUrl { get; set; } = "https://api.tme.eu/";

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }
    }
}
