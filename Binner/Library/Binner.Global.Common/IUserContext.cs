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
        /// Get the subscription level of the user
        /// </summary>
        public SubscriptionLevel SubscriptionLevel { get; }

        /// <summary>
        /// Custom properties for the user
        /// </summary>
        IDictionary<string, object?> Properties { get; set; }

        /// <summary>
        /// Returns if Property exists and is not null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HasProperty(string key);

        /// <summary>
        /// Returns if boolean Property is true or not.
        /// </summary>
        /// <param name="key">Name of property</param>
        /// <returns></returns>
        bool IsPropertyTrue(string key);
    }
}
