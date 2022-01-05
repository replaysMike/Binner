namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// Standard 2 of 5 encoding
    /// </summary>
    class Standard2of5 : BarcodeCommon, IBarcode
    {
        private readonly string[] _s25Code = { "10101110111010", "11101010101110", "10111010101110", "11101110101010", "10101110101110", "11101011101010", "10111011101010", "10101011101110", "11101010111010", "10111010111010" };
        private readonly BarcodeType _encodedType = BarcodeType.Unspecified;

        public Standard2of5(string input, BarcodeType encodedType)
        {
            RawData = input;
            _encodedType = encodedType;
        }

        /// <summary>
        /// Encode the raw data using the Standard 2 of 5 algorithm.
        /// </summary>
        private string EncodeStandard2of5()
        {
            if (!CheckNumericOnly(RawData))
                Error("ES25-1: Numeric Data Only");

            var result = "11011010";

            for (var i = 0; i < RawData.Length; i++)
            {
                result += _s25Code[(int)char.GetNumericValue(RawData, i)];
            }

            result += _encodedType == BarcodeType.Standard2of5_Mod10 ? _s25Code[CalculateMod10CheckDigit()] : "";

            // add ending bars
            result += "1101011";
            return result;
        }

        private int CalculateMod10CheckDigit()
        {
            var sum = 0;
            var even = true;
            for (var i = RawData.Length - 1; i >= 0; --i)
            {
                // convert numeric in char format to integer and
                // multiply by 3 or 1 based on if an even index from the end
                sum += (RawData[i] - '0') * (even ? 3 : 1);
                even = !even;
            }

            return (10 - sum % 10) % 10;
        }

        #region IBarcode Members

        public string Encoded_Value => EncodeStandard2of5();

        #endregion

    }
}
