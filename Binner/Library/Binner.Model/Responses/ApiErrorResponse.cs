namespace Binner.Model.Responses
{
    public class ApiErrorResponse
    {
        /// <summary>
        /// The error message
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Error returned by the api
        /// </summary>
        public string? ApiMessage { get; set; }

        public int StatusCode { get; set; }

        public ApiErrorResponse(string error)
        {
            Error = error;
        }

        public ApiErrorResponse(string error, string apiMessage, int statusCode)
        {
            Error = error;
            ApiMessage = apiMessage;
            StatusCode = statusCode;
        }
    }
}
