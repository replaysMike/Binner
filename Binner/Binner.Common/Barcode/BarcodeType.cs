namespace Binner.Common.Barcode
{
    /// <summary>
    /// The available barcode types
    /// </summary>
    public enum BarcodeType : int
    {
        Unspecified, 
        UPCA, 
        UPCE, 
        UPC_SUPPLEMENTAL_2DIGIT, 
        UPC_SUPPLEMENTAL_5DIGIT, 
        EAN13, 
        EAN8, 
        Interleaved2of5, 
        Interleaved2of5_Mod10, 
        Standard2of5, 
        Standard2of5_Mod10, 
        Industrial2of5, 
        Industrial2of5_Mod10, 
        CODE39, 
        CODE39Extended, 
        CODE39_Mod43, 
        Codabar, 
        PostNet, 
        BOOKLAND, 
        ISBN, 
        JAN13, 
        MSI_Mod10, 
        MSI_2Mod10, 
        MSI_Mod11, 
        MSI_Mod11_Mod10, 
        Modified_Plessey, 
        CODE11, 
        USD8, 
        UCC12, 
        UCC13, 
        LOGMARS, 
        CODE128, 
        CODE128A, 
        CODE128B, 
        CODE128C, 
        ITF14, 
        CODE93, 
        TELEPEN, 
        FIM, 
        PHARMACODE
    };
}