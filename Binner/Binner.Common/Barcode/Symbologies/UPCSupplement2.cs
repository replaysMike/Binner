namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// UPC Supplement-2 encoding
    /// </summary>
    public class UpcSupplement2 : BarcodeSymbology
    {
        private readonly string [] _eanCodeA    = { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011" };
        private readonly string [] _eanCodeB    = { "0100111", "0110011", "0011011", "0100001", "0011101", "0111001", "0000101", "0010001", "0001001", "0010111" };
        private readonly string[] _upcSupp2 = { "aa", "ab", "ba", "bb" };

        public UpcSupplement2(string input)
        {
            RawData = input;
        }

        /// <summary>
        /// Encode the raw data using the UPC Supplemental 2-digit algorithm.
        /// </summary>
        private string EncodeUPCSupplemental2()
        {
            if (RawData.Length != 2) Error("EUPC-SUP2-1: Invalid data length. (Length = 2 required)");

            if (!CheckNumericOnly(RawData))
                Error("EUPC-SUP2-2: Numeric Data Only");

            var pattern = "";

            try
            {
                pattern = _upcSupp2[int.Parse(RawData.Trim()) % 4];
            }
            catch { Error("EUPC-SUP2-3: Invalid Data. (Numeric only)"); }

            var result = "1011";

            var pos = 0;
            foreach (char c in pattern)
            {
                if (c == 'a')
                {
                    // encode using odd parity
                    result += _eanCodeA[int.Parse(RawData[pos].ToString())];
                }
                else if (c == 'b')
                {
                    // encode using even parity
                    result += _eanCodeB[int.Parse(RawData[pos].ToString())];
                }

                if (pos++ == 0) result += "01"; //Inter-character separator
            }
            return result;
        }

        #region IBarcode Members

        public override string EncodedValue => EncodeUPCSupplemental2();

        #endregion

    }
}
