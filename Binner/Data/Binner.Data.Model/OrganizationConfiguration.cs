using Binner.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// Stores user defined printer configurations
    /// </summary>
    public class OrganizationConfiguration : IEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrganizationConfigurationId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// Use the Binner module to configure the UI
        /// </summary>
        public BinnerModules UseModule { get; set; }

        /// <summary>
        /// If you have a paid Binner Cloud subscription, provide your license key to activate pro features
        /// </summary>
        public string? LicenseKey { get; set; }

        /// <summary>
        /// Maximum number of items that can be cached
        /// </summary>
        public int MaxCacheItems { get; set; } = 1024;

        /// <summary>
        /// Sliding cache expiration in minutes
        /// </summary>
        public int CacheSlidingExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Absolute cache expiration in minutes (0 = never)
        /// </summary>
        public int CacheAbsoluteExpirationMinutes { get; set; } = 0;

        /// <summary>
        /// KiCad settings serialized as JSON
        /// </summary>
        public string? KiCadSettingsJson { get; set; }

        /// <summary>
        /// If true, allows fetching part metadata for parts that already exist in inventory
        /// </summary>
        public bool enableAutomaticMetadataFetchingForExistingParts { get; set; } = true;

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization? Organization { get; set; }
    }
}
