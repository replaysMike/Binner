using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// A user context
    /// </summary>
    public class User : IEntity, IUserData
    {
        /// <summary>
        /// User Id
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        /// <summary>
        /// Name of user
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address of user
        /// </summary>
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Phone number of user
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Hashed password
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Set if the account is locked and cannot login
        /// </summary>
        public DateTime? DateLockedUtc { get; set; }

        /// <summary>
        /// True if email address is confirmed
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        /// Date the user confirmed their email
        /// </summary>
        public DateTime? DateEmailConfirmedUtc { get; set; }

        /// <summary>
        /// True if email address is subscribed to extra emails
        /// </summary>
        public bool IsEmailSubscribed { get; set; }

        /// <summary>
        /// True if user is an admin account
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Email address token
        /// </summary>
        public string? EmailConfirmationToken { get; set; }

        /// <summary>
        /// User's profile image
        /// </summary>
        public byte[]? ProfileImage { get; set; }

        /// <summary>
        /// Date the user last logged in
        /// </summary>
        public DateTime? DateLastLoginUtc { get; set; }

        /// <summary>
        /// Date the user last made a request from the api
        /// </summary>
        public DateTime? DateLastActiveUtc { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        /// <summary>
        /// The score of ReCaptcha when the user signed up
        /// </summary>
        public double? ReCaptchaScore { get; set; }

        /// <summary>
        /// Ip address who created the account
        /// </summary>
        public long Ip { get; set; }

        /// <summary>
        /// Ip address who confirmed the email address
        /// </summary>
        public long EmailConfirmedIp { get; set; }

        /// <summary>
        /// Ip address who last set the password
        /// </summary>
        public long LastSetPasswordIp { get; set; }

        public ICollection<UserToken>? UserTokens { get; set; }

        public ICollection<UserLoginHistory>? UserLoginHistory { get; set; }

        public ICollection<Part>? Parts { get; set; }

        public ICollection<PartType>? PartTypes { get; set; }

        public ICollection<Project>? Projects { get; set; }

        public ICollection<OAuthCredential>? OAuthCredentials { get; set; }

        public ICollection<OAuthRequest>? OAuthRequests { get; set; }

        public ICollection<PartSupplier>? PartSuppliers { get; set; }
        
        public ICollection<UserIntegrationConfiguration>? UserIntegrationConfigurations { get; set; }

        public ICollection<UserPrinterConfiguration>? UserPrinterConfigurations { get; set; }

        public ICollection<UserPrinterTemplateConfiguration>? UserPrinterTemplateConfigurations { get; set; }
    }
}