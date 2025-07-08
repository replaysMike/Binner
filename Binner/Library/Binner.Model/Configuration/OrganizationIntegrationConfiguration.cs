using Binner.Model.Configuration.Integrations;

namespace Binner.Model.Configuration
{
    public class OrganizationIntegrationConfiguration
    {
        /// <summary>
        /// Binner Swarm api enabled
        /// </summary>
        public bool SwarmEnabled { get; set; } = true;

        /// <summary>
        /// The Api key
        /// </summary>
        public string? SwarmApiKey { get; set; }

        /// <summary>
        /// The Api url
        /// </summary>
        public string? SwarmApiUrl { get; set; } = "https://swarm.binner.io";

        /// <summary>
        /// The timeout
        /// </summary>
        public TimeSpan? SwarmTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Digikey api enabled
        /// </summary>
        public bool DigiKeyEnabled { get; set; }

        /// <summary>
        /// DigiKey site to use
        /// </summary>
        public DigikeyLocaleSite DigiKeySite { get; set; } = DigikeyLocaleSite.US;

        /// <summary>
        /// The Client Id
        /// </summary>
        public string? DigiKeyClientId { get; set; }

        /// <summary>
        /// The Client Secret
        /// </summary>
        public string? DigiKeyClientSecret { get; set; }

        /// <summary>
        /// The oAuth postback url
        /// </summary>
        public string? DigiKeyOAuthPostbackUrl { get; set; } = "https://localhost:8090/Authorization/Authorize";

        /// <summary>
        /// The api url
        /// </summary>
        public string? DigiKeyApiUrl { get; set; } = "https://sandbox-api.digikey.com";

        /// <summary>
        /// Mouser api enabled
        /// </summary>
        public bool MouserEnabled { get; set; }

        /// <summary>
        /// The Api key for searches
        /// </summary>
        public string? MouserSearchApiKey { get; set; }

        /// <summary>
        /// The Api key for order management
        /// </summary>
        public string? MouserOrderApiKey { get; set; }

        /// <summary>
        /// The Api key for cart management
        /// </summary>
        public string? MouserCartApiKey { get; set; }

        /// <summary>
        /// The Api url
        /// </summary>
        public string? MouserApiUrl { get; set; } = "https://api.mouser.com";

        /// <summary>
        /// Arrow api enabled
        /// </summary>
        public bool ArrowEnabled { get; set; }

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
        /// Octopart/Nexar api enabled
        /// </summary>
        public bool NexarEnabled { get; set; } = false;

        /// <summary>
        /// Octopart/Nexar Client Id
        /// </summary>
        public string? NexarClientId { get; set; }

        /// <summary>
        /// Octopart/Nexar Client Secret
        /// </summary>
        public string? NexarClientSecret { get; set; }

        /// <summary>
        /// TME api enabled
        /// </summary>
        public bool TmeEnabled { get; set; }

        /// <summary>
        /// The user's TME country
        /// </summary>
        public string? TmeCountry { get; set; } = "us";

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
        public string TmeApiUrl { get; set; } = "https://api.tme.eu";

        /// <summary>
        /// True to resolve external (document) links. This can slow down responses
        /// </summary>
        public bool TmeResolveExternalLinks { get; set; } = true;
    }
}
