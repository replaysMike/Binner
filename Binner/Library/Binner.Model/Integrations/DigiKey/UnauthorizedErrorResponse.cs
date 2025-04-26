namespace Binner.Model.Integrations.DigiKey
{
    /// <summary>
    /// An error response message on Unauthorized (401) responses
    /// </summary>
    public class UnauthorizedErrorResponse
    {
        public string? ErrorResponseVersion { get; set; }
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetails { get; set; }
        public Guid RequestId { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }
}
