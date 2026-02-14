namespace Binner.Global.Common
{
    public interface IUser : ICustomFields
    {
        /// <summary>
        /// User Id
        /// </summary>
        int UserId { get; set; }

        /// <summary>
        /// Organization Id
        /// </summary>
        int OrganizationId { get; set; }

        /// <summary>
        /// Name of user
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Email address of user
        /// </summary>
        string EmailAddress { get; set; }

        string? Password { get; set; }

        /// <summary>
        /// Phone number of user
        /// </summary>
        string? PhoneNumber { get; set; }

        /// <summary>
        /// Set if the account is locked and cannot login
        /// </summary>
        DateTime? DateLockedUtc { get; set; }

        /// <summary>
        /// True if email address is confirmed
        /// </summary>
        bool IsEmailConfirmed { get; set; }

        /// <summary>
        /// Date the user confirmed their email
        /// </summary>
        DateTime? DateEmailConfirmedUtc { get; set; }

        /// <summary>
        /// True if user is an admin account
        /// </summary>
        bool IsAdmin { get; set; }

        /// <summary>
        /// Email address token
        /// </summary>
        string? EmailConfirmationToken { get; set; }

        /// <summary>
        /// Total number of parts in inventory
        /// </summary>
        int PartsInventoryCount { get; set; }

        /// <summary>
        /// Total number of custom part types
        /// </summary>
        int PartTypesCount { get; set; }

        /// <summary>
        /// User's profile image
        /// </summary>
        string? ProfileImage { get; set; }

        /// <summary>
        /// Ip address
        /// </summary>
        string? IpAddress { get; set; }

        /// <summary>
        /// The user's preferred language, used for APIs
        /// </summary>
        string? LocaleLanguage { get; set; }

        /// <summary>
        /// The user's preferred currency, used for APIs and display
        /// </summary>
        string? LocaleCurrency { get; set; }

        DateTime DateCreatedUtc { get; set; }

        DateTime DateModifiedUtc { get; set; }

        DateTime? DateLastLoginUtc { get; set; }

        DateTime? DateLastActiveUtc { get; set; }

        Guid GlobalId { get; set; }
    }
}
