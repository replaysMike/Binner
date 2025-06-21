using Binner.Global.Common;
using Binner.Model.Requests;
using Binner.Model.Responses;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Binner.Services
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticate using user password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);

        /// <summary>
        /// Get authentication by refresh token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<AuthenticationResponse> RefreshTokenAsync(string token);
        
        /// <summary>
        /// Revoke a refresh token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task RevokeTokenAsync(string token);
        
        /// <summary>
        /// Get a user by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserContext?> GetUserAsync(int userId);

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<RegisterNewAccountResponse> RegisterNewAccountAsync(RegisterNewAccountRequest request);

        /// <summary>
        /// Set the current request/thread User from a userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>The created claims principal</returns>
        Task<ClaimsPrincipal> SetCurrentUserFromIdAsync(int userId);

        /// <summary>
        /// Send a password recovery request email to the user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PasswordRecoveryResponse> SendPasswordResetRequest(PasswordRecoveryRequest request);

        /// <summary>
        /// Validate a password reset token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PasswordRecoveryResponse> ValidatePasswordResetTokenAsync(ConfirmPasswordRecoveryRequest request);

        /// <summary>
        /// Reset a user's password using a password reset token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AuthenticationResponse> ResetPasswordUsingTokenAsync(PasswordRecoverySetNewPasswordRequest request);
    }
}
