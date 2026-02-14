using Binner.Global.Common;

namespace Binner.Model
{
    /// <summary>
    /// User account
    /// </summary>
    public class User : IUser, ICustomFields
    {
        /// <summary>
        /// User Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Organization Id
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Name of user
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address of user
        /// </summary>
        public string EmailAddress { get; set; } = string.Empty;

        public string? Password { get; set; }

        /// <summary>
        /// Phone number of user
        /// </summary>
        public string? PhoneNumber { get; set; }

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
        /// True if user is an admin account
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Email address token
        /// </summary>
        public string? EmailConfirmationToken { get; set; }

        /// <summary>
        /// Total number of parts in inventory
        /// </summary>
        public int PartsInventoryCount { get; set; }

        /// <summary>
        /// Total number of custom part types
        /// </summary>
        public int PartTypesCount { get; set; }

        /// <summary>
        /// User's profile image
        /// </summary>
        public string? ProfileImage { get; set; }

        /// <summary>
        /// Ip address
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// The user's preferred language, used for APIs
        /// </summary>
        public string? LocaleLanguage { get; set; }

        /// <summary>
        /// The user's preferred currency, used for APIs and display
        /// </summary>
        public string? LocaleCurrency { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        public DateTime? DateLastLoginUtc { get; set; }

        public DateTime? DateLastActiveUtc { get; set; }

        public ICollection<CustomValue> CustomFields { get; set; } = new List<CustomValue>();

        public Guid GlobalId { get; set; }
    }
}
