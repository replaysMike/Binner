using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Binner.Web.Authorization
{
    /// <summary>
    /// KiCad token based authorization
    /// </summary>
    /// <remarks>
    /// An "Authorization" header will be passed a token. Ex: headers["AUTHORIZATION"] = 'Token 123456789ABC'
    /// via https://dev-docs.kicad.org/en/apis-and-binding/http-libraries/index.html
    /// </remarks>
    public class KiCadTokenAuthorizeAttribute : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
    {
        const string HEADER_NAME = "AUTHORIZATION";
        const string TOKEN_NAME = "Token";

        /// <summary>
        /// The name of the authentication header, default: "AUTHORIZATION"
        /// </summary>
        public string HeaderName { get; init; }

        /// <summary>
        /// The name of the authentication token, default: "Token"
        /// </summary>
        public string TokenName { get; init; }

        public KiCadTokenAuthorizeAttribute(string headerName = HEADER_NAME, string tokenName = TOKEN_NAME)
        {
            HeaderName = headerName;
            TokenName = tokenName;
        }

        public IEnumerable<IAuthorizationRequirement> GetRequirements()
        {
            yield return this;
        }
    }
}
