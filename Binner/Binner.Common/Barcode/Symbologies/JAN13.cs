namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// JAN-13 encoding
    /// </summary>
    public class JAN13 : BarcodeCommon, IBarcode
    {
        public JAN13(string input)
        {
            Raw_Data = input;
        }

        /// <summary>
        /// Encode the raw data using the JAN-13 algorithm.
        /// </summary>
        private string EncodeJAN13()
        {
            if (!Raw_Data.StartsWith("49")) Error("EJAN13-1: Invalid Country Code for JAN13 (49 required)");
            if (!CheckNumericOnly(Raw_Data))
                Error("EJAN13-2: Numeric Data Only");

            var ean13 = new EAN13(Raw_Data);
            return ean13.Encoded_Value;
        }

        #region IBarcode Members

        public string Encoded_Value => EncodeJAN13();

        #endregion

    }
}
