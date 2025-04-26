using Binner.Common.Integrations.Models;
using System;

namespace Binner.Common.Integrations
{
    public class ApiErrorException : Exception
    {
        public IApiResponse ApiResponse { get; set; }

        public ApiErrorException(IApiResponse apiResponse, string message) : base(message)
        {
            ApiResponse = apiResponse;
        }
    }
}
