namespace Binner.Model
{
    /// <summary>
    /// License key information
    /// </summary>
    public class SubscriptionLicenseKey
    {
        public int SubscriptionId { get; set; }

        /// <summary>
        /// The license key associated with the subscription
        /// </summary>
        public string LicenseKey { get; set; } = string.Empty;

        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        /// <summary>
        /// Date the license key is active for
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Date the license key is no longer active after and must be renewed
        /// </summary>
        public DateTime PeriodEnd { get; set; }
    }
}
