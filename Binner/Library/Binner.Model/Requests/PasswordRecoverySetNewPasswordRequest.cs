using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Requests
{
    public class PasswordRecoverySetNewPasswordRequest
    {
        /// <summary>
        /// Username
        /// </summary>
        [Required]
        public string EmailAddress { get; set; } = null!;

        /// <summary>
        /// Password recovery token
        /// </summary>
        [Required]
        public string Token { get; set; } = null!;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
