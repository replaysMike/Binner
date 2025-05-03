using Binner.Model.Barcode;

namespace Binner.Model.Requests
{
    public class CreateBulkPartRequest
    {
        public ICollection<BulkPart>? Parts { get; set; }
    }

    public class BulkPart : PartBase
    {
        public int OriginalQuantity { get; set; }
        public int ScannedQuantity { get; set; }
        public string? SupplierPartNumber { get; set; }
        public BarcodeScan? BarcodeObject { get; set; }
    }
}
