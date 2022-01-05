namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// UPC Supplement-5 encoding
    /// </summary>
    public class UPCSupplement5 : BarcodeSymbology, IBarcode
    {
        private readonly string[] _eanCodeA = { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011" };
        private readonly string[] _eanCodeB = { "0100111", "0110011", "0011011", "0100001", "0011101", "0111001", "0000101", "0010001", "0001001", "0010111" };
        private readonly string[] _upcSupp5 = { "bbaaa", "babaa", "baaba", "baaab", "abbaa", "aabba", "aaabb", "ababa", "abaab", "aabab" };

        public UPCSupplement5(string input)
        {
            RawData = input;
        }

        /// <summary>
        /// Encode the raw data using the UPC Supplemental 5-digit algorithm.
        /// </summary>
        private string EncodeUPCSupplemental5()
        {
            if (RawData.Length != 5) Error("EUPC-SUP5-1: Invalid data length. (Length = 5 required)");

            if (!CheckNumericOnly(RawData))
                Error("EUPCA-2: Numeric Data Only");

            // calculate the checksum digit
            var even = 0;
            var odd = 0;

            // odd
            for (var i = 0; i <= 4; i += 2)
            {
                odd += int.Parse(RawData.Substring(i, 1)) * 3;
            }

            // even
            for (var i = 1; i < 4; i += 2)
            {
                even += int.Parse(RawData.Substring(i, 1)) * 9;
            }

            var total = even + odd;
            var cs = total % 10;

            var pattern = _upcSupp5[cs];

            var result = "";

            var pos = 0;
            foreach (var c in pattern)
            {
                // Inter-character separator
                if (pos == 0) result += "1011";
                else result += "01";

                switch (c)
                {
                    case 'a':
                        // encode using odd parity
                        result += _eanCodeA[int.Parse(RawData[pos].ToString())]; //if
                        break;
                    case 'b':
                        // encode using even parity
                        result += _eanCodeB[int.Parse(RawData[pos].ToString())]; //else if  
                        break;
                }
                pos++;
            }
            return result;
        }

        #region IBarcode Members

        public string Encoded_Value => EncodeUPCSupplemental5();

        #endregion

    }
}
