namespace Binner.Model.Responses
{
    public class PrintSpoolQueueResponse
    {
        public ICollection<PrintSpoolQueue> Queue { get; set; } = new List<PrintSpoolQueue>();

        /// <summary>
        /// The crc of the printer configuration.
        /// If it doesn't match, a new configuration must be fetched.
        /// </summary>
        public int PrinterConfigurationCrc { get; set; }
    }
}
