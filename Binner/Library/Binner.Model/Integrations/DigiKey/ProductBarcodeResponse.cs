namespace Binner.Model.Integrations.DigiKey
{
    public class ProductBarcodeResponse
    {
        public string? DigiKeyPartNumber { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public string? ManufacturerName { get; set; }
        public string? ProductDescription { get; set; }
        public int Quantity { get; set; }
    }
}
