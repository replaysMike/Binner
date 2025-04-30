namespace Binner.Model.Integrations.DigiKey
{
    /// <summary>
    /// A DigiKey error response message
    /// </summary>
    public class ErrorResponse
    {
        public string? ErrorResponseVersion { get; set; }
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetails { get; set; }
        public Guid RequestId { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }
}
