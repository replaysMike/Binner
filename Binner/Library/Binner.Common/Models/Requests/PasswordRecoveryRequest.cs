using System.ComponentModel.DataAnnotations;

namespace Binner.Common.Models.Requests
{
    public class PasswordRecoveryRequest
    {
        /// <summary>
        /// Username
        /// </summary>
        [Required]
        public string EmailAddress { get; set; } = null!;
    }
}
