using Binner.Model;
using System;

namespace Binner.Common.Integrations
{
    /// <summary>
    /// Occurs when the Digikey api key used is not subscribed for the endpoint
    /// </summary>
    public class DigikeyUnsubscribedException : Exception
    {
        public OAuthAuthorization Authorization { get; }
        public string ApiVersion { get; }

        public DigikeyUnsubscribedException(OAuthAuthorization authorization, string apiVersion, string message) : base(message)
        {
            Authorization = authorization;
            ApiVersion = apiVersion;
        }
    }
}
