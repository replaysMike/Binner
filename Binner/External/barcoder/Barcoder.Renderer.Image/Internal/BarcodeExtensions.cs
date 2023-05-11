namespace Barcoder.Renderer.Image.Internal
{
    internal static class BarcodeExtensions
    {
        public static bool IsEanBarcode(this IBarcode barcode)
            => barcode?.Metadata.CodeKind == BarcodeType.EAN8 || barcode?.Metadata.CodeKind == BarcodeType.EAN13;
    }
}
