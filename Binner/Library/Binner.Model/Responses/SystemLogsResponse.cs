namespace Binner.Model.Responses
{
    public class SystemLogsResponse
    {
        public string LogEntry { get; } = string.Empty;
        public SystemLogsResponse(string logEntry)
        {
            LogEntry = logEntry;
        }
    }
}
