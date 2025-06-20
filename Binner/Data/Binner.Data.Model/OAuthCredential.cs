using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class OAuthCredential
#if INITIALCREATE
        : IEntity,
#else
        : IPartialEntity,
#endif
        IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OAuthCredentialId { get; set; }

        /// <summary>
        /// The provider credential
        /// </summary>
        [MaxLength(128)]
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// The access token
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// The refresh token
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// The date the Access token was created
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// The date the Access token will expire
        /// </summary>
        public DateTime DateExpiresUtc { get; set; }

        /// <summary>
        /// Storage for additional api settings related to a credential
        /// </summary>
        public string? ApiSettings { get; set; }

#if INITIALCREATE
        /// <summary>
        /// Ip address who created the request
        /// </summary>
        public long Ip { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
#endif
    }
}
