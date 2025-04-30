namespace Binner.Model.Integrations.DigiKey
{
    /// <summary>
    /// The api settings json object stored in OAuthCredentials.ApiSettings
    /// </summary>
    public class DigiKeyCredentialApiSettings
    {
        public DigiKeyApiVersion ApiVersion { get; set; } = DigiKeyApiVersion.V3;
    }
}
