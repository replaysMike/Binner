using Binner.Model;
using Binner.Model.Integrations.DigiKey;
using System;

namespace Binner.Common.Integrations
{
    /// <summary>
    /// Occurs when the Digikey api key used is not subscribed for the endpoint
    /// </summary>
    public class DigikeyUnsubscribedException : Exception
    {
        public OAuthAuthorization Authorization { get; }
        public DigiKeyApiVersion ApiVersion { get; }
        public string? ErrorResponseVersion { get; }

        public DigikeyUnsubscribedException(OAuthAuthorization authorization, DigiKeyApiVersion apiVersion, string? errorResponseVersion, string message) : base(message)
        {
            Authorization = authorization;
            ApiVersion = apiVersion;
            ErrorResponseVersion = errorResponseVersion;
        }
    }
}
