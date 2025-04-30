using Binner.Model;
using System;

namespace Binner.Common.Integrations
{
    /// <summary>
    /// Occurs when an oAuth token used is invalid or expired
    /// </summary>
    public class DigikeyUnauthorizedException : Exception
    {
        public OAuthAuthorization Authorization { get; }

        public DigikeyUnauthorizedException(OAuthAuthorization authorization) : base("User must authorize")
        {
            Authorization = authorization;
        }
    }
}
