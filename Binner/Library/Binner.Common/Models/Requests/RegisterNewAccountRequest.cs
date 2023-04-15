using System.ComponentModel.DataAnnotations;

namespace Binner.Common.Models.Requests
{
    public class RegisterNewAccountRequest
    {
        /// <summary>
        /// Full name
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        [Required]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
