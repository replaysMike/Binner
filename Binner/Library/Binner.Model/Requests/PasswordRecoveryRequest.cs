using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Requests
{
    public class PasswordRecoveryRequest
    {
        /// <summary>
        /// Username
        /// </summary>
        [Required]
        public string EmailAddress { get; set; } = null!;
        
        /// <summary>
        /// ReCaptcha token
        /// </summary>
        public string? Token { get;set;}
    }
}
