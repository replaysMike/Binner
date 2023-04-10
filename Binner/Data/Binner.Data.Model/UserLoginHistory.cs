using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// A user login entry
    /// </summary>
    public class UserLoginHistory
    {
        /// <summary>
        /// User Id
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserLoginHistoryId { get; set; }

        /// <summary>
        /// User who logged in
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Email address used to login
        /// </summary>
        public string? EmailAddress { get; set; }

        /// <summary>
        /// True if authentication was successful
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// True if user was allowed to login
        /// </summary>
        public bool CanLogin { get; set; }

        /// <summary>
        /// Error message if applicable
        /// </summary>
        public string? Message { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// The ReCaptcha score during login
        /// </summary>
        public double? ReCaptchaScore { get; set; }

        /// <summary>
        /// Ip address who created the account
        /// </summary>
        public long Ip { get; set; }


        public User? User { get; set; }

    }
}
