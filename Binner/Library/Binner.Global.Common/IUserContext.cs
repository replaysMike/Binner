namespace Binner.Global.Common
{
    public interface IUserContext
    {
        /// <summary>
        /// User Id
        /// </summary>
        int UserId { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        int OrganizationId { get; set; }

        /// <summary>
        /// Name of user
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// Email address of user
        /// </summary>
        string? EmailAddress { get; set; }

        /// <summary>
        /// Phone number of user
        /// </summary>
        string? PhoneNumber { get; set; }

        /// <summary>
        /// False to disable logins
        /// </summary>
        bool CanLogin { get; set; }

        /// <summary>
        /// Is Admin
        /// </summary>
        bool IsAdmin { get; set; }

        /// <summary>
        /// Custom properties for the user
        /// </summary>
        IDictionary<string, object?> Properties { get; set; }
    }
}
