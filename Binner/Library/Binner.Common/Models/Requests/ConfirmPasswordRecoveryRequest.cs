using System.ComponentModel.DataAnnotations;

namespace Binner.Common.Models.Requests
{
    public class ConfirmPasswordRecoveryRequest
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
    }
}
