using System.ComponentModel.DataAnnotations;

namespace Binner.Model
{
    public class OAuthCredential : IEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int OAuthCredentialId { get; set; }

        /// <summary>
        /// The provider credential
        /// </summary>
        [MaxLength(32)]
        public string? Provider { get; set; }

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
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date last modified
        /// </summary>
        public DateTime DateModifiedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date the Access token will expire
        /// </summary>
        public DateTime DateExpiresUtc { get; set; }

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }
    }
}
