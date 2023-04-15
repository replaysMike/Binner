using Binner.Model.Authentication;

namespace Binner.Common.Models.Responses
{
    public class AuthenticationResponse : AuthenticatedTokens
    {
        /// <summary>
        /// User Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of user
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Friendly message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// True if user is an admin
        /// </summary>
        public bool IsAdmin { get; set; }

        public AuthenticationResponse(bool isAuthenticated, string message)
        {
            IsAuthenticated = isAuthenticated;
            Message = message;
        }

        public AuthenticationResponse(UserContext user, AuthenticatedTokens authenticatedTokens)
        {
            Id = user.UserId;
            Name = user.Name;
            IsAuthenticated = authenticatedTokens.IsAuthenticated;
            JwtToken = authenticatedTokens.JwtToken;
            ImagesToken = authenticatedTokens.ImagesToken;
            RefreshToken = authenticatedTokens.RefreshToken;
            CanLogin = authenticatedTokens.CanLogin;
            IsAdmin = user.IsAdmin;
        }
    }
}
