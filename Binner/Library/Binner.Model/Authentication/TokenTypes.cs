namespace Binner.Model.Authentication
{
    /// <summary>
    /// Identifies an authentication token type
    /// </summary>
    public enum TokenTypes
    {
        /// <summary>
        /// Access token (short-lived)
        /// </summary>
        AccessToken = 0,
        /// <summary>
        /// Refresh token (long-lived)
        /// </summary>
        RefreshToken,
        /// <summary>
        /// Token used to validate emails
        /// </summary>
        EmailConfirmationToken,
        /// <summary>
        /// Password reset token
        /// </summary>
        PasswordResetToken,
        /// <summary>
        /// Images access token
        /// </summary>
        ImagesToken,
        /// <summary>
        /// KiCad api access token
        /// </summary>
        KiCadApiToken
    }
}
