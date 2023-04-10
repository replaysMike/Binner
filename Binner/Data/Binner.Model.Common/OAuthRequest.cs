using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Common
{
    public class OAuthRequest : IEntity, IEquatable<OAuthRequest>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int OAuthRequestId { get; set; }

        public Guid RequestId { get; set; }

        /// <summary>
        /// Name of the Api provider
        /// </summary>
        public string Provider { get; set; } = null!;

        /// <summary>
        /// True if an oAuth callback was received from Digikey
        /// </summary>
        public bool AuthorizationReceived { get; set; }

        /// <summary>
        /// Authorization code
        /// </summary>
        public string? AuthorizationCode { get; set; }

        /// <summary>
        /// Error definition
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Error description
        /// </summary>
        public string? ErrorDescription { get; set; }

        /// <summary>
        /// The url to return to after authorization is performed
        /// </summary>
        public string? ReturnToUrl { get; set; }

        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        public DateTime DateModifiedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is OAuthRequest credential)
                return Equals(credential);
            return false;
        }

        public bool Equals(OAuthRequest? other)
        {
            return other != null && Provider == other.Provider && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return OAuthRequestId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(OAuthRequestId, UserId != null ? UserId : 0);
#endif

        }

        public override string ToString()
        {
            return $"{Provider}";
        }
    }
}
