namespace Binner.Common.IO.Printing
{
    public class PrinterResult
    {
        /// <summary>
        /// True if the print was successful
        /// </summary>
        public bool IsSuccess { get; internal set; }

        /// <summary>
        /// The error message
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// The error code returned by the printer
        /// </summary>
        public int ErrorCode { get; internal set; }

    }
}
