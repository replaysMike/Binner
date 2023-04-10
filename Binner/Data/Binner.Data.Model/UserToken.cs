using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Binner.Model.Authentication;

namespace Binner.Data.Model
{
    /// <summary>
    /// Tracks different kinds of authentication tokens
    /// </summary>
    public class UserToken : IEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserTokenId { get; set; }

        /// <summary>
        /// User associated with the token
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The type of token
        /// </summary>
        public TokenTypes TokenTypeId { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// The token that replaced this expired token
        /// </summary>
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// The date the token expires
        /// </summary>
        public DateTime? DateExpiredUtc { get; set; }

        /// <summary>
        /// The date the token was revoked
        /// </summary>
        public DateTime? DateRevokedUtc { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        /// <summary>
        /// Ip that created the token
        /// </summary>
        public long Ip { get; set; }

        public User? User { get; set; }
    }
}
