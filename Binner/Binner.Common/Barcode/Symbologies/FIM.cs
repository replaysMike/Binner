namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// FIM encoding
    /// </summary>
    public class FIM: BarcodeCommon, IBarcode
    {
        private readonly string[] FIMCodes = { "110010011", "101101101", "110101011", "111010111" };
        public enum FIMTypes {FIM_A = 0, FIM_B, FIM_C, FIM_D};

        public FIM(string input)
        {
            input = input.Trim();

            switch (input)
            {
                case "A":
                case "a": RawData = FIMCodes[(int)FIMTypes.FIM_A];
                    break;
                case "B":
                case "b": RawData = FIMCodes[(int)FIMTypes.FIM_B];
                    break;
                case "C":
                case "c": RawData = FIMCodes[(int)FIMTypes.FIM_C];
                    break;
                case "D":
                case "d": RawData = FIMCodes[(int)FIMTypes.FIM_D];
                    break;
                default: Error("EFIM-1: Could not determine encoding type. (Only pass in A, B, C, or D)");
                    break;
            }
        }

        public string Encode_FIM()
        {
            var encoded = "";
            foreach (char c in RawData)
            {
                encoded += c + "0";
            }

            encoded = encoded.Substring(0, encoded.Length - 1);

            return encoded;
        }

        #region IBarcode Members

        public string Encoded_Value => Encode_FIM();

        #endregion

    }
}
