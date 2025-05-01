using Binner.Model;
using Binner.Model.Integrations.DigiKey;
using System;

namespace Binner.Common.Integrations
{
    /// <summary>
    /// Occurs when an oAuth token used is invalid or expired
    /// </summary>
    public class DigikeyUnauthorizedException : Exception
    {
        public OAuthAuthorization Authorization { get; }
        public DigiKeyApiVersion ApiVersion { get; }

        public DigikeyUnauthorizedException(OAuthAuthorization authorization, DigiKeyApiVersion apiVersion) : base("User must authorize")
        {
            Authorization = authorization;
            ApiVersion = apiVersion;
        }
    }
}
