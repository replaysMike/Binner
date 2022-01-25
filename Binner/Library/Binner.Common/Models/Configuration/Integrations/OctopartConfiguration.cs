namespace Binner.Common.Models.Configuration
{
    public class OctopartConfiguration
    {
        public string ApiKey { get; set; }

        /// <summary>
        /// Path to the Octopart Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://octopart.com";
    }
}
