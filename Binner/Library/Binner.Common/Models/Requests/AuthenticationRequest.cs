using System.ComponentModel.DataAnnotations;

namespace Binner.Common.Models.Requests
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
        [Required]
        public string Password { get; set; } = null!;
    }
}
