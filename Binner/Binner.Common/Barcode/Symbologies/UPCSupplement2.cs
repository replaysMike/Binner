using System;

namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// UPC Supplement-2 encoding
    /// </summary>
    public class UPCSupplement2 : BarcodeCommon, IBarcode
    {
        private readonly string [] EAN_CodeA    = { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011" };
        private readonly string [] EAN_CodeB    = { "0100111", "0110011", "0011011", "0100001", "0011101", "0111001", "0000101", "0010001", "0001001", "0010111" };
        private readonly string[] UPC_SUPP_2 = { "aa", "ab", "ba", "bb" };

        public UPCSupplement2(string input)
        {
            Raw_Data = input;
        }

        /// <summary>
        /// Encode the raw data using the UPC Supplemental 2-digit algorithm.
        /// </summary>
        private string EncodeUPCSupplemental2()
        {
            if (Raw_Data.Length != 2) Error("EUPC-SUP2-1: Invalid data length. (Length = 2 required)");

            if (!CheckNumericOnly(Raw_Data))
                Error("EUPC-SUP2-2: Numeric Data Only");

            string pattern = "";

            try
            {
                pattern = this.UPC_SUPP_2[Int32.Parse(Raw_Data.Trim()) % 4];
            }
            catch { Error("EUPC-SUP2-3: Invalid Data. (Numeric only)"); }

            string result = "1011";

            int pos = 0;
            foreach (char c in pattern)
            {
                if (c == 'a')
                {
                    // encode using odd parity
                    result += EAN_CodeA[Int32.Parse(Raw_Data[pos].ToString())];
                }
                else if (c == 'b')
                {
                    // encode using even parity
                    result += EAN_CodeB[Int32.Parse(Raw_Data[pos].ToString())];
                }

                if (pos++ == 0) result += "01"; //Inter-character separator
            }
            return result;
        }

        #region IBarcode Members

        public string Encoded_Value => EncodeUPCSupplemental2();

        #endregion

    }
}
