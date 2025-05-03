using AnyBarcode;
using Binner.Common;

namespace Binner.Model.Requests
{
    public class CreatePartScanHistoryRequest
    {
        public long? PartId { get; set; }

        /// <summary>
        /// The raw scan string
        /// </summary>
        public string RawScan { get; set; } = null!;

        /// <summary>
        /// A crc of the RawScan value
        /// </summary>
        public int Crc { get; set; }

        public BarcodeType BarcodeType { get; set; }

        public ScannedLabelType ScannedLabelType { get; set; }

        public Suppliers Supplier { get; set; }

        public string? ManufacturerPartNumber { get; set; }

        public string? SupplierPartNumber { get; set; }

        public string? SalesOrder { get; set; }

        public string? Invoice { get; set; }

        public int Quantity { get; set; }

        public string? Mid { get; set; }

        public string? LotCode { get; set; }

        public string? Description { get; set; }

        public string? CountryOfOrigin { get; set; }

        public string? Packlist { get; set; }
    }
}
