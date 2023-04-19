namespace Binner.Model
{
    /// <summary>
    /// Account of User
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Name of user
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address of user
        /// </summary>
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// The user's current password, required if changing password
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// The new password
        /// </summary>
        public string? NewPassword { get; set; }

        /// <summary>
        /// Confirm the new password
        /// </summary>
        public string? ConfirmNewPassword { get; set; }

        /// <summary>
        /// Phone number of user
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// True if email address is confirmed
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        /// Date the user confirmed their email
        /// </summary>
        public DateTime? DateEmailConfirmedUtc { get; set; }

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
        /// The user's current ip address, if applicable
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// The user's preferred language, used for APIs
        /// </summary>
        public string? LocaleLanguage { get; set; }

        /// <summary>
        /// The user's preferred currency, used for APIs and display
        /// </summary>
        public string? LocaleCurrency { get; set; }
    }
}
