namespace Binner.Model.Integrations.DigiKey
{
    /// <summary>
    /// An error response message on Unauthorized (401) responses
    /// </summary>
    public class UnauthorizedTokenResponse
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int Status { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }
        public Guid CorrelationId { get; set; }
        public ErrorObject Errors { get; set; } = new();
    }

    // this object isn't document, and never seems to have a value
    public class ErrorObject { }
}
