using Binner.Model.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
#if INITIALCREATE
    /// <summary>
    /// Stores user defined printer configurations
    /// </summary>
    public class UserBarcodeConfiguration : IEntity, IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserBarcodeConfigurationId { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// True to enable barcode scanning features
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Enabling Barcode debug mode will print diagnostic information to the browser console
        /// </summary>
        public bool IsDebug { get; set; } = false;

        /// <summary>
        /// The maximum amount of time in milliseconds to wait between keypresses.
        /// </summary>
        public int MaxKeystrokeThresholdMs { get; set; } = 300;

        /// <summary>
        /// Set the buffer time in milliseconds used to filter out barcode commands. Default: 80
        /// </summary>
        public int BufferTime { get; set; } = 80;

        /// <summary>
        /// Set the 2D barcode prefix, usually [)>
        /// </summary>
        public string Prefix2D { get; set; } = @"[)>";

        /// <summary>
        /// The barcode scanner profile to use
        /// </summary>
        public BarcodeProfiles Profile { get; set; } = BarcodeProfiles.Default;

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
