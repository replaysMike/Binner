namespace Binner.Model.Responses
{
    /// <summary>
    /// An error response generated via an Exception
    /// </summary>
    public class ExceptionResponse
    {
        /// <summary>
        /// The error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The type of exception that occurred
        /// </summary>
        public string ExceptionType { get;set;}

        /// <summary>
        /// The exception stack trace
        /// </summary>
        public string? StackTrace { get; set; }

        public ExceptionResponse(Exception exception) : this(string.Empty, exception) { }

        public ExceptionResponse(string message, Exception exception)
        {
            ExceptionType = $"{exception.GetType().FullName}";
            Message = $"{message}{Environment.NewLine}";
            Message += $"An exception occurred of type {ExceptionType}{Environment.NewLine}";
            Message += $"{exception.Message}{Environment.NewLine}";
            if (exception.InnerException != null)
            {
                Message += $"Base Exception:{Environment.NewLine}{exception.GetBaseException().Message}{Environment.NewLine}";
            }
            StackTrace = exception.StackTrace;
        }
    }
}
