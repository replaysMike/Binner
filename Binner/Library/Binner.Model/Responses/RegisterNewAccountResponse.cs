using Binner.Model.Authentication;

namespace Binner.Model.Responses
{
    public class RegisterNewAccountResponse : AuthenticatedTokens
    {
        /// <summary>
        /// User Id
        /// </summary>
        public int Id { get; set; }
        
        public string Name { get; set; }

        /// <summary>
        /// True if operation was successful
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// User account information
        /// </summary>
        public UserContext? User { get; set; }

        public RegisterNewAccountResponse() { }

        public RegisterNewAccountResponse(AuthenticatedTokens authenticatedTokens)
        {
            IsAuthenticated = authenticatedTokens.IsAuthenticated;
            JwtToken = authenticatedTokens.JwtToken;
            ImagesToken = authenticatedTokens.ImagesToken;
            RefreshToken = authenticatedTokens.RefreshToken;
        }
    }
}
