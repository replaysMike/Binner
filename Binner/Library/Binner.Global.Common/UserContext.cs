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

        /// <summary>
        /// Get the subscription level of the user
        /// </summary>
        public SubscriptionLevel SubscriptionLevel { get; internal set; }

        /// <summary>
        /// Custom properties for the user
        /// </summary>
        public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();

        /// <summary>
        /// Returns if Property exists and is not null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasProperty(string key)
            => Properties.ContainsKey(key) && Properties[key] != null;

        /// <summary>
        /// Returns if boolean Property is true or not.
        /// </summary>
        /// <param name="key">Name of property</param>
        /// <returns></returns>
        public bool IsPropertyTrue(string key)
            => Properties.ContainsKey(key) && bool.Parse(Properties[key]?.ToString() ?? "false");
    }
}
