namespace Binner.Model.Barcode
{
    /// <summary>
    /// The available barcode types
    /// </summary>
    public enum BarcodeTypes : int
    {
        Unspecified,
        Datamatrix,
        Upca,
        Upce,
        UpcSupplemental2Digit,
        UpcSupplemental5Digit,
        Ean13,
        Ean8,
        Interleaved2of5,
        Interleaved2of5Mod10,
        Standard2of5,
        Standard2of5Mod10,
        Industrial2of5,
        Industrial2of5Mod10,
        Code39,
        Code39Extended,
        Code9Mod43,
        Codabar,
        PostNet,
        Bookland,
        Isbn,
        Jan13,
        MsiMod10,
        Msi2Mod10,
        MsiMod11,
        MsiMod11Mod10,
        ModifiedPlessey,
        Code11,
        Usd8,
        Ucci12,
        Ucci13,
        LogMars,
        Code128,
        Code128A,
        Code128B,
        Code128C,
        Itf14,
        Code93,
        Telepen,
        Fim,
        PharmaCode
    };

    public static class BarcodeTypesHelper
    {
        public static BarcodeTypes GetBarcodeType(string str)
        {
            var barcodeType = BarcodeTypes.Unspecified;
            Enum.TryParse<BarcodeTypes>(str, true, out barcodeType);
            return barcodeType;
        }
    }
}
