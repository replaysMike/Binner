namespace Binner.Model.Authentication
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
        /// User's subscription level
        /// </summary>
        public const string SubscriptionLevel = "SubscriptionLevel";

        /// <summary>
        /// The user is allowed to login
        /// </summary>
        public const string CanLogin = "CanLogin";

        public const string Admin = "Admin";
    }
}
