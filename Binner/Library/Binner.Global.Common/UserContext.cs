namespace Binner.Global.Common
{
    /// <summary>
    /// A user context
    /// </summary>
    public class UserContext : IUserContext
    {
        /// <summary>
        /// User Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Name of user
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Email address of user
        /// </summary>
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Phone number of user
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// False to disable logins
        /// </summary>
        public bool CanLogin { get; set; }

        /// <summary>
        /// Is Admin
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}
