namespace Binner.Model.Responses
{
    /// <summary>
    /// An licensing error response generated via an Exception
    /// </summary>
    public class LicenseResponse
    {
        /// <summary>
        /// The error message
        /// </summary>
        public string Message { get; set; }

        public LicenseResponse(Exception exception)
        {
            Message = exception.Message;
        }

        public LicenseResponse(string message, Exception exception)
        {
            Message = message + Environment.NewLine;
            Message += exception.Message;
        }
    }
}
