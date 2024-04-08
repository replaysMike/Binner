using Binner.Common.Integrations.Models;
using System;

namespace Binner.Common.Integrations
{
    public class ApiRequiresAuthenticationException : Exception
    {
        public IApiResponse ApiResponse { get; set; }

        public ApiRequiresAuthenticationException(IApiResponse apiResponse)
        {
            ApiResponse = apiResponse;
        }
    }
}
