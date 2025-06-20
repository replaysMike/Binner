using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Requests
{
    public class AuthenticationRequest
    {
        /// <summary>
        /// Username
        /// </summary>
        [Required]
        public string Username { get; set; } = null!;

        /// <summary>
        /// Clear-text password
        /// </summary>
        public string? Password { get; set; } = null!;

        /// <summary>
        /// The ReCaptcha or Turnstile token
        /// </summary>
        public string? Token { get; set; }
    }
}
