namespace Binner.Model
{
    public class OAuthRequest
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int OAuthRequestId { get; set; }

        public Guid RequestId { get; set; }

        /// <summary>
        /// Name of the Api provider
        /// </summary>
        public string Provider { get; set; } = null!;

        /// <summary>
        /// User Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// True if an oAuth callback was received from Digikey
        /// </summary>
        public bool AuthorizationReceived { get; set; }

        /// <summary>
        /// Authorization code
        /// </summary>
        public string? AuthorizationCode { get; set; }

        /// <summary>
        /// Error definition
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Error description
        /// </summary>
        public string? ErrorDescription { get; set; }

        /// <summary>
        /// The url to return to after authorization is performed
        /// </summary>
        public string? ReturnToUrl { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }
    }
}
