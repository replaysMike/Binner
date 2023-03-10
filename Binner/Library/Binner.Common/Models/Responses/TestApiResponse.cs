namespace Binner.Common.Models.Responses
{
    public class TestApiResponse
    {
        public string ApiName { get; set; } = null!;
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AuthorizationUrl { get; set; }

        public TestApiResponse(string apiName, string? message)
        {
            ApiName = apiName;
            Message = message;
        }

        public TestApiResponse(string apiName, bool success)
        {
            ApiName = apiName;
            Success = success;
        }
    }
}
