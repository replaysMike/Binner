using Newtonsoft.Json;

namespace Binner.Model.Authentication
{
    public class ReCaptchaResponse
    {
        /// <summary>
        /// whether this request was a valid reCAPTCHA token for your site
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ)
        /// </summary>
        [JsonProperty("challenge_ts")]
        public DateTime Challenge_ts { get; set; }

        /// <summary>
        /// the hostname of the site where the reCAPTCHA was solved
        /// </summary>
        [JsonProperty("hostname")]
        public string? Hostname { get; set; }

        /// <summary>
        /// the score for this request (0.0 - 1.0)
        /// </summary>
        [JsonProperty("score")]
        public double Score { get; set; }

        /// <summary>
        /// the action name for this request (important to verify)
        /// </summary>
        [JsonProperty("action")]
        public string? Action { get; set; }

        /// <summary>
        /// optional errors
        /// </summary>
        [JsonProperty("error-codes")]
        public IEnumerable<string?>? ErrorCodes { get; set; }

        public override string ToString()
            => $" Success: {Success} Score: {Score} Action: {Action} Hostname: {Hostname} Challenge_ts: {Challenge_ts} ErrorCodes: {(ErrorCodes != null ? string.Join(",", ErrorCodes) : "")}";
    }
}
