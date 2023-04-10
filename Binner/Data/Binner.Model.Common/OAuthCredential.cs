using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Common
{
    public class OAuthCredential : IEntity, IEquatable<OAuthCredential>
    {
        /// <summary>
        /// The provider credential
        /// </summary>
        [Key, MaxLength(32)]
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
        /// The date the Access token will expire
        /// </summary>
        public DateTime DateExpiresUtc { get; set; }

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is OAuthCredential credential)
                return Equals(credential);
            return false;
        }

        public bool Equals(OAuthCredential? other)
        {
            return other != null && Provider == other.Provider && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return Provider.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(Provider, UserId);
#endif

        }

        public override string ToString()
        {
            return $"{Provider}";
        }
    }
}
