namespace Binner.Model.Responses
{
    public class ValidateLicenseKeyResponse
    {
        /// <summary>
        /// True if the license is valid
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// An optional response message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The license information
        /// </summary>
        public SubscriptionLicenseKey? License { get; set; }

        public ValidateLicenseKeyResponse()
        {
        }

        public ValidateLicenseKeyResponse(string message)
        {
            IsValidated = false;
            Message = message;
        }

        public ValidateLicenseKeyResponse(SubscriptionLicenseKey? license)
        {
            IsValidated = license != null ? true : false;
            License = license;
        }
    }
}
