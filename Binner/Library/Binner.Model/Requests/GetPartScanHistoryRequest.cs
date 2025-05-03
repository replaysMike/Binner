namespace Binner.Model.Requests
{
    public class GetPartScanHistoryRequest
    {
        /// <summary>
        /// The project id
        /// </summary>
        public long PartScanHistoryId { get; set; }

        /// <summary>
        /// Raw barcode scan string
        /// </summary>
        public string? RawScan { get; set; }

        /// <summary>
        /// 32bit crc of the RawScan value
        /// </summary>
        public int Crc { get; set; }
    }
}
