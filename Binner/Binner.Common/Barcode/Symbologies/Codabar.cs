namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// Codabar encoding
    /// </summary>
    public class Codabar: BarcodeCommon, IBarcode
    {
        private readonly System.Collections.Hashtable Codabar_Code = new();
        
        public Codabar(string input)
        {
            Raw_Data = input;
        }

        /// <summary>
        /// Encode the raw data using the Codabar algorithm.
        /// </summary>
        private string EncodeCodabar()
        {
            if (Raw_Data.Length < 2) Error("ECODABAR-1: Data format invalid. (Invalid length)");

            // check first char to make sure its a start/stop char
            switch (Raw_Data[0].ToString().ToUpper().Trim())
            {
                case "A": break;
                case "B": break;
                case "C": break;
                case "D": break;
                default: Error("ECODABAR-2: Data format invalid. (Invalid START character)");
                    break;
            }

            // check the ending char to make sure its a start/stop char
            switch (Raw_Data[Raw_Data.Trim().Length - 1].ToString().ToUpper().Trim())
            {
                case "A": break;
                case "B": break;
                case "C": break;
                case "D": break;
                default: Error("ECODABAR-3: Data format invalid. (Invalid STOP character)");
                    break;
            }

            // populate the hashtable to begin the process
            InitCodabar();

            // replace non-numeric VALID chars with empty strings before checking for all numerics
            var temp = Raw_Data;

            foreach (char c in Codabar_Code.Keys)
            {
                if (!CheckNumericOnly(c.ToString()))
                {
                    temp = temp.Replace(c, '1');
                }
            }

            // now that all the valid non-numeric chars have been replaced with a number check if all numeric exist
            if (!CheckNumericOnly(temp))
                Error("ECODABAR-4: Data contains invalid  characters.");

            var result = "";

            foreach (var c in Raw_Data)
            {
                result += Codabar_Code[c].ToString();
                result += "0"; // inter-character space
            }

            // remove the extra 0 at the end of the result
            result = result.Remove(result.Length - 1);

            // clears the hashtable so it no longer takes up memory
            Codabar_Code.Clear();

            // change the Raw_Data to strip out the start stop chars for label purposes
            Raw_Data = Raw_Data.Trim().Substring(1, RawData.Trim().Length - 2);

            return result;
        }

        private void InitCodabar()
        {
            Codabar_Code.Clear();
            Codabar_Code.Add('0', "101010011");//"101001101101");
            Codabar_Code.Add('1', "101011001");//"110100101011");
            Codabar_Code.Add('2', "101001011");//"101100101011");
            Codabar_Code.Add('3', "110010101");//"110110010101");
            Codabar_Code.Add('4', "101101001");//"101001101011");
            Codabar_Code.Add('5', "110101001");//"110100110101");
            Codabar_Code.Add('6', "100101011");//"101100110101");
            Codabar_Code.Add('7', "100101101");//"101001011011");
            Codabar_Code.Add('8', "100110101");//"110100101101");
            Codabar_Code.Add('9', "110100101");//"101100101101");
            Codabar_Code.Add('-', "101001101");//"110101001011");
            Codabar_Code.Add('$', "101100101");//"101101001011");
            Codabar_Code.Add(':', "1101011011");//"110110100101");
            Codabar_Code.Add('/', "1101101011");//"101011001011");
            Codabar_Code.Add('.', "1101101101");//"110101100101");
            Codabar_Code.Add('+', "101100110011");//"101101100101");
            Codabar_Code.Add('A', "1011001001");//"110110100101");
            Codabar_Code.Add('B', "1010010011");//"101011001011");
            Codabar_Code.Add('C', "1001001011");//"110101100101");
            Codabar_Code.Add('D', "1010011001");//"101101100101");
            Codabar_Code.Add('a', "1011001001");//"110110100101");
            Codabar_Code.Add('b', "1010010011");//"101011001011");
            Codabar_Code.Add('c', "1001001011");//"110101100101");
            Codabar_Code.Add('d', "1010011001");//"101101100101");
        }

        #region IBarcode Members

        public string Encoded_Value => EncodeCodabar();

        #endregion

    }
}
