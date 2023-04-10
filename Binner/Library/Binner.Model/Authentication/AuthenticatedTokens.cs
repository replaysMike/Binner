namespace Binner.Model.Authentication
{
    public class AuthenticatedTokens
    {
        /// <summary>
        /// True if the user is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// True if user can login
        /// </summary>
        public bool CanLogin { get; set; }

        /// <summary>
        /// Jwt access token
        /// </summary>
        public string? JwtToken { get; set; }

        /// <summary>
        /// A token that can be used to access images securely
        /// </summary>
        public string? ImagesToken { get; set; }

        /// <summary>
        /// Refresh token created date
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Refresh token expires date
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public DateTime DateExpires { get; set; }

        /// <summary>
        /// Jwt refresh token
        /// </summary>
        /// <remarks>
        /// Never return this via the Api, should only be returned as an http-only cookie
        /// </remarks>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string RefreshToken { get; set; } = null!;
    }
}
