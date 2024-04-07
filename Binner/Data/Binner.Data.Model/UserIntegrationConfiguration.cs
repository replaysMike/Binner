using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
#if INITIALCREATE
    /// <summary>
    /// Stores user defined integration configurations
    /// </summary>
    public class UserIntegrationConfiguration : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserIntegrationConfigurationId { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

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
        public string? SwarmApiUrl { get; set; }

        /// <summary>
        /// The timeout
        /// </summary>
        public TimeSpan? SwarmTimeout { get; set; }

        /// <summary>
        /// Digikey api enabled
        /// </summary>
        public bool DigiKeyEnabled { get; set; } = true;

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
        public string? DigiKeyOAuthPostbackUrl { get; set; }

        /// <summary>
        /// The api url
        /// </summary>
        public string? DigiKeyApiUrl { get; set; }

        /// <summary>
        /// Mouser api enabled
        /// </summary>
        public bool MouserEnabled { get; set; } = true;

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
        public string? MouserApiUrl { get; set; }

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
        /// Octopart api enabled
        /// </summary>
        public bool OctopartEnabled { get; set; } = false;

        /// <summary>
        /// Octopart Client Id
        /// </summary>
        public string? OctopartClientId { get; set; }

        /// <summary>
        /// Octopart Client Secret
        /// </summary>
        public string? OctopartClientSecret { get; set; }

        /// <summary>
        /// TME api enabled
        /// </summary>
        public bool TmeEnabled { get; set; } = true;

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
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
#endif
}
