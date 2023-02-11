namespace Binner.Common.Models.Configuration.Integrations
{
    /// <summary>
    /// Digikey APi configuration settings
    /// </summary>
    public class DigikeyConfiguration
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Digikey Client Id
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Digikey Client Secret
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// The oAuth Postback Url - this must match the Callback Url for the App you configured on Digikey's API
        /// </summary>
        public string oAuthPostbackUrl { get; set; } = "http://localhost:8090/Authorization/Authorize";

        /// <summary>
        /// Path to the Digikey Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://sandbox-api.digikey.com";
    }
}
