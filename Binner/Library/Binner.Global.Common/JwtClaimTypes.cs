namespace Binner.Global.Common
{
    public static class JwtClaimTypes
    {
        /// <summary>
        /// User's full name
        /// </summary>
        public const string FullName = "FullName";

        /// <summary>
        /// User's internal UserId
        /// </summary>
        public const string UserId = "UserId";

        /// <summary>
        /// User's internal OrganizationId
        /// </summary>
        public const string OrganizationId = "OrganizationId";

        /// <summary>
        /// User's subscription level
        /// </summary>
        public const string SubscriptionLevel = "SubscriptionLevel";

        /// <summary>
        /// The user is allowed to login
        /// </summary>
        public const string CanLogin = "CanLogin";

        /// <summary>
        /// User has organization admin status
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// User's super admin status which can manage all users of the system
        /// </summary>
        public const string SuperAdmin = "SuperAdmin";
    }
}
