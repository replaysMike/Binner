namespace Binner.Model.Authentication
{
    public static class AuthorizationPolicies
    {
        /// <summary>
        /// Requires a Maker subscription policy
        /// </summary>
        public const string MakerSubscription = "MakerSubscription";

        /// <summary>
        /// Requires a Professional subscription policy
        /// </summary>
        public const string ProfessionalSubscription = "ProfessionalSubscription";

        /// <summary>
        /// Requires an Admin policy
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// Requires an SuperAdmin policy
        /// </summary>
        public const string SuperAdmin = "SuperAdmin";

        /// <summary>
        /// Requires a login policy
        /// </summary>
        public const string CanLogin = "CanLogin";
    }
}
