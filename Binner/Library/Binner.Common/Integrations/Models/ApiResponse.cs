using System.Collections.Generic;
using System.Linq;

namespace Binner.Common.Integrations.Models
{
    /// <summary>
    /// An api response
    /// </summary>
    public class ApiResponse : IApiResponse
    {
        /// <summary>
        /// Response from the api
        /// </summary>
        public object? Response { get; set; }

        /// <summary>
        /// Requires authentication to continue
        /// </summary>
        public bool RequiresAuthentication { get; set; }

        /// <summary>
        /// A redirect Url location if requires authentication
        /// </summary>
        public string? RedirectUrl { get; set; }

        /// <summary>
        /// Errors
        /// </summary>
        public ICollection<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// Warnings
        /// </summary>
        public ICollection<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Name of api
        /// </summary>
        public string ApiName { get; set; }

        public ApiResponse(string apiName)
        {
            ApiName = apiName;
        }

        public ApiResponse(object response, string apiName)
        {
            Response = response;
            ApiName = apiName;
        }

        public ApiResponse(ICollection<string> errors, string apiName)
        {
            Errors = errors;
            ApiName = apiName;
        }

        public ApiResponse(bool requiresAuthentication, string redirectUrl, ICollection<string> errors, string apiName)
        {
            RequiresAuthentication = requiresAuthentication;
            RedirectUrl = redirectUrl;
            Errors = errors;
            ApiName = apiName;
        }

        public static ApiResponse Create(object response, string apiName)
        {
            return new ApiResponse(response, apiName);
        }

        public static ApiResponse Create(string error, string apiName)
        {
            return new ApiResponse(Enumerable.Repeat(error, 1), apiName);
        }

        public static ApiResponse CreateWarning(string warning, string apiName)
        {
            return new ApiResponse(apiName)
            {
                Warnings = Enumerable.Repeat(warning, 1).ToList()
            };
        }

        public static ApiResponse Create(ICollection<string> errors, string apiName)
        {
            return new ApiResponse(errors, apiName);
        }

        public static ApiResponse CreateWarnings(ICollection<string> warnings, string apiName)
        {
            return new ApiResponse(apiName)
            {
                Warnings = warnings
            };
        }

        public static ApiResponse Create(bool requiresAuthentication, string redirectUrl, string error, string apiName)
        {
            return new ApiResponse(requiresAuthentication, redirectUrl, Enumerable.Repeat(error, 1).ToList(), apiName);
        }

        public static ApiResponse Create(bool requiresAuthentication, string redirectUrl, ICollection<string> errors, string apiName)
        {
            return new ApiResponse(requiresAuthentication, redirectUrl, errors, apiName);
        }
    }
}
