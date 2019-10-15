//-----------------------------------------------------------------------
//
// THE SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES OF ANY KIND, EXPRESS, IMPLIED, STATUTORY, 
// OR OTHERWISE. EXPECT TO THE EXTENT PROHIBITED BY APPLICABLE LAW, DIGI-KEY DISCLAIMS ALL WARRANTIES, 
// INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, 
// SATISFACTORY QUALITY, TITLE, NON-INFRINGEMENT, QUIET ENJOYMENT, 
// AND WARRANTIES ARISING OUT OF ANY COURSE OF DEALING OR USAGE OF TRADE. 
// 
// DIGI-KEY DOES NOT WARRANT THAT THE SOFTWARE WILL FUNCTION AS DESCRIBED, 
// WILL BE UNINTERRUPTED OR ERROR-FREE, OR FREE OF HARMFUL COMPONENTS.
// 
//-----------------------------------------------------------------------

using System;

namespace ApiClient.Constants
{
    /// <summary>
    ///     Uri constants to talk to our OAuth2 server implementation.
    /// </summary>
    public static class DigiKeyUriConstants
    {
        // Production Sandbox instance
        public static readonly Uri BaseAddress = new Uri("https://sandbox-api.digikey.com");
        public static readonly Uri TokenEndpoint = new Uri("https://sandbox-api.digikey.com/v1/oauth2/token");
        public static readonly Uri AuthorizationEndpoint = new Uri("https://sandbox-api.digikey.com/v1/oauth2/authorize");

        // Production instance
        //public static readonly Uri BaseAddress = new Uri("https://api.digikey.com");
        //public static readonly Uri TokenEndpoint = new Uri("https://api.digikey.com/v1/oauth2/token");
        //public static readonly Uri AuthorizationEndpoint = new Uri("https://api.digikey.com/v1/oauth2/authorize");
    }
}
