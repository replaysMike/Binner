namespace Binner.Model.Requests
{
    public class GetPartScanHistoryRequest
    {
        /// <summary>
        /// The part scan history id
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

        /// <summary>
        /// True to generate a crc based on Rawscan and search the crc
        /// </summary>
        public bool SearchCrc { get; set; }
    }
}
