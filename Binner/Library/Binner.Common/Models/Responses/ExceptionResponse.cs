using System;

namespace Binner.Common.Models.Responses
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
        public string StackTrace { get; set; }

        public ExceptionResponse(Exception exception) : this(string.Empty, exception) { }

        public ExceptionResponse(string message, Exception exception)
        {
            ExceptionType = $"{exception.GetType().FullName}";
            Message = $"{message}An exception occurred of type {ExceptionType}{Environment.NewLine}";
            Message += $"{exception.Message}";
            if (exception.InnerException != null)
            {
                Message += $"{Environment.NewLine}Base Exception:{Environment.NewLine}{exception.GetBaseException().Message}";
            }
            StackTrace = exception.StackTrace;
        }
    }
}
