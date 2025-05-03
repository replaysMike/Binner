using Binner.Model.Barcode;

namespace Binner.Model.Requests
{
    public class CreatePartRequest : PartBase
    {
        public BarcodeScan? BarcodeObject { get; set; }
    }
}
