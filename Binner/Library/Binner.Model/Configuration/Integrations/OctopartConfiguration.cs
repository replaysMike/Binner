namespace Binner.Model.Configuration
{
    public class OctopartConfiguration : IApiConfiguration
    {
        public bool Enabled { get; set; } = false;

        public string ApiKey => ClientId ?? string.Empty;

        /// <summary>
        /// Client Id
        /// </summary>
        public string? ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Client Secret
        /// </summary>
        public string? ClientSecret { get; set; } = string.Empty;

        public bool IsConfigured => Enabled && !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret);
    }
}
