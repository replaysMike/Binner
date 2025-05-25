using Binner.Common;

namespace Binner.Model.Barcode
{
    public class BarcodeScan
    {
        public string Type { get; set; } = null!;
        public object? Value { get; set; } // can be string or Dictionary<string, object?>
        public ScannedLabelType ScannedLabelType { get; set; } = ScannedLabelType.Unknown;
        public Suppliers Supplier { get; set; } = Suppliers.Unknown;
        public string? CorrectedValue { get; set; }
        public string RawValue { get; set; } = null!;
        public bool RsDetected { get; set; }
        public bool GsDetected { get; set; }
        public bool EotDetected { get; set; }
        public bool InvalidBarcodeDetected { get; set; }
        public string? RawValueFormatted { get; set; }
    }
}
