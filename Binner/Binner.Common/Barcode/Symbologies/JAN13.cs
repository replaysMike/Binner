namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// JAN-13 encoding
    /// </summary>
    public class Jan13 : BarcodeSymbology
    {
        public Jan13(string input)
        {
            RawData = input;
        }

        /// <summary>
        /// Encode the raw data using the JAN-13 algorithm.
        /// </summary>
        private string EncodeJAN13()
        {
            if (!RawData.StartsWith("49")) Error("EJAN13-1: Invalid Country Code for JAN13 (49 required)");
            if (!CheckNumericOnly(RawData))
                Error("EJAN13-2: Numeric Data Only");

            var ean13 = new Ean13(RawData);
            return ean13.EncodedValue;
        }

        #region IBarcode Members

        public override string EncodedValue => EncodeJAN13();

        #endregion

    }
}
