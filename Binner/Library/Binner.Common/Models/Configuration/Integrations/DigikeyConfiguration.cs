namespace Binner.Common.Models.Configuration.Integrations
{
    /// <summary>
    /// Digikey APi configuration settings
    /// </summary>
    public class DigikeyConfiguration : IApiConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string? ApiKey => ClientId;

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
        // ReSharper disable once InconsistentNaming
        public string oAuthPostbackUrl { get; set; } = "https://localhost:8090/Authorization/Authorize";

        /// <summary>
        /// Path to the Digikey Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.digikey.com";

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret) && !string.IsNullOrEmpty(oAuthPostbackUrl) && !string.IsNullOrEmpty(ApiUrl);
    }
}
