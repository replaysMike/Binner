using Binner.Model;
using Binner.Model.Integrations.DigiKey;
using System;

namespace Binner.Common.Integrations
{
    /// <summary>
    /// Occurs when the Digikey api ClientId is invalid
    /// </summary>
    public class DigikeyInvalidCredentialsException : Exception
    {
        public OAuthAuthorization Authorization { get; }
        public DigiKeyApiVersion ApiVersion { get; }
        public string? ErrorResponseVersion { get; }

        public DigikeyInvalidCredentialsException(OAuthAuthorization authorization, DigiKeyApiVersion apiVersion, string? errorResponseVersion, string message) : base(message)
        {
            Authorization = authorization;
            ApiVersion = apiVersion;
            ErrorResponseVersion = errorResponseVersion;
        }
    }
}
