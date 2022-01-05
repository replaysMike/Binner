using System;

namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// Code 39 encoding
    /// </summary>
    public class Code39 : BarcodeSymbology, IBarcode
    {
        private readonly System.Collections.Hashtable C39Code = new();
        private readonly System.Collections.Hashtable ExtC39Translation = new();
        private readonly bool _allowExtended;
        private readonly bool _enableChecksum;

        /// <summary>
        /// Encodes with Code39.
        /// </summary>
        /// <param name="input">Data to encode.</param>
        public Code39(string input)
        {
            RawData = input;
        }

        /// <summary>
        /// Encodes with Code39.
        /// </summary>
        /// <param name="input">Data to encode.</param>
        /// <param name="allowExtended">Allow Extended Code 39 (Full Ascii mode).</param>
        public Code39(string input, bool allowExtended)
        {
            RawData = input;
            _allowExtended = allowExtended;
        }

        /// <summary>
        /// Encodes with Code39.
        /// </summary>
        /// <param name="input">Data to encode.</param>
        /// <param name="allowExtended">Allow Extended Code 39 (Full Ascii mode).</param>
        /// <param name="enableChecksum">Whether to calculate the Mod 43 checksum and encode it into the barcode</param>
        public Code39(string input, bool allowExtended, bool enableChecksum)
        {
            RawData = input;
            _allowExtended = allowExtended;
            _enableChecksum = enableChecksum;
        }

        /// <summary>
        /// Encode the raw data using the Code 39 algorithm.
        /// </summary>
        private string EncodeCode39()
        {
            InitCode39();
            InitExtendedCode39();

            var strNoAstr = RawData.Replace("*", "");
            var strFormattedData = "*" + strNoAstr + (_enableChecksum ? GetChecksumChar(strNoAstr).ToString() : String.Empty) + "*";

            if (_allowExtended)
                InsertExtendedCharsIfNeeded(ref strFormattedData);

            var result = "";
            foreach (var c in strFormattedData)
            {
                try
                {
                    result += C39Code[c].ToString();
                    result += "0"; // whitespace
                }
                catch
                {
                    if (_allowExtended)
                        Error("EC39-1: Invalid data.");
                    else
                        Error("EC39-1: Invalid data. (Try using Extended Code39)");
                }
            }

            result = result.Substring(0, result.Length-1);
            
            // clear the hashtable so it no longer takes up memory
            C39Code.Clear();

            return result;
        }
        private void InitCode39()
        {
            C39Code.Clear();
            C39Code.Add('0', "101001101101");
            C39Code.Add('1', "110100101011");
            C39Code.Add('2', "101100101011");
            C39Code.Add('3', "110110010101");
            C39Code.Add('4', "101001101011");
            C39Code.Add('5', "110100110101");
            C39Code.Add('6', "101100110101");
            C39Code.Add('7', "101001011011");
            C39Code.Add('8', "110100101101");
            C39Code.Add('9', "101100101101");
            C39Code.Add('A', "110101001011");
            C39Code.Add('B', "101101001011");
            C39Code.Add('C', "110110100101");
            C39Code.Add('D', "101011001011");
            C39Code.Add('E', "110101100101");
            C39Code.Add('F', "101101100101");
            C39Code.Add('G', "101010011011");
            C39Code.Add('H', "110101001101");
            C39Code.Add('I', "101101001101");
            C39Code.Add('J', "101011001101");
            C39Code.Add('K', "110101010011");
            C39Code.Add('L', "101101010011");
            C39Code.Add('M', "110110101001");
            C39Code.Add('N', "101011010011");
            C39Code.Add('O', "110101101001");
            C39Code.Add('P', "101101101001");
            C39Code.Add('Q', "101010110011");
            C39Code.Add('R', "110101011001");
            C39Code.Add('S', "101101011001");
            C39Code.Add('T', "101011011001");
            C39Code.Add('U', "110010101011");
            C39Code.Add('V', "100110101011");
            C39Code.Add('W', "110011010101");
            C39Code.Add('X', "100101101011");
            C39Code.Add('Y', "110010110101");
            C39Code.Add('Z', "100110110101");
            C39Code.Add('-', "100101011011");
            C39Code.Add('.', "110010101101");
            C39Code.Add(' ', "100110101101");
            C39Code.Add('$', "100100100101");
            C39Code.Add('/', "100100101001");
            C39Code.Add('+', "100101001001");
            C39Code.Add('%', "101001001001");
            C39Code.Add('*', "100101101101");
        }

        private void InitExtendedCode39()
        {
            ExtC39Translation.Clear();
            ExtC39Translation.Add(Convert.ToChar(0).ToString(), "%U");
            ExtC39Translation.Add(Convert.ToChar(1).ToString(), "$A");
            ExtC39Translation.Add(Convert.ToChar(2).ToString(), "$B");
            ExtC39Translation.Add(Convert.ToChar(3).ToString(), "$C");
            ExtC39Translation.Add(Convert.ToChar(4).ToString(), "$D");
            ExtC39Translation.Add(Convert.ToChar(5).ToString(), "$E");
            ExtC39Translation.Add(Convert.ToChar(6).ToString(), "$F");
            ExtC39Translation.Add(Convert.ToChar(7).ToString(), "$G");
            ExtC39Translation.Add(Convert.ToChar(8).ToString(), "$H");
            ExtC39Translation.Add(Convert.ToChar(9).ToString(), "$I");
            ExtC39Translation.Add(Convert.ToChar(10).ToString(), "$J");
            ExtC39Translation.Add(Convert.ToChar(11).ToString(), "$K");
            ExtC39Translation.Add(Convert.ToChar(12).ToString(), "$L");
            ExtC39Translation.Add(Convert.ToChar(13).ToString(), "$M");
            ExtC39Translation.Add(Convert.ToChar(14).ToString(), "$N");
            ExtC39Translation.Add(Convert.ToChar(15).ToString(), "$O");
            ExtC39Translation.Add(Convert.ToChar(16).ToString(), "$P");
            ExtC39Translation.Add(Convert.ToChar(17).ToString(), "$Q");
            ExtC39Translation.Add(Convert.ToChar(18).ToString(), "$R");
            ExtC39Translation.Add(Convert.ToChar(19).ToString(), "$S");
            ExtC39Translation.Add(Convert.ToChar(20).ToString(), "$T");
            ExtC39Translation.Add(Convert.ToChar(21).ToString(), "$U");
            ExtC39Translation.Add(Convert.ToChar(22).ToString(), "$V");
            ExtC39Translation.Add(Convert.ToChar(23).ToString(), "$W");
            ExtC39Translation.Add(Convert.ToChar(24).ToString(), "$X");
            ExtC39Translation.Add(Convert.ToChar(25).ToString(), "$Y");
            ExtC39Translation.Add(Convert.ToChar(26).ToString(), "$Z");
            ExtC39Translation.Add(Convert.ToChar(27).ToString(), "%A");
            ExtC39Translation.Add(Convert.ToChar(28).ToString(), "%B");
            ExtC39Translation.Add(Convert.ToChar(29).ToString(), "%C");
            ExtC39Translation.Add(Convert.ToChar(30).ToString(), "%D");
            ExtC39Translation.Add(Convert.ToChar(31).ToString(), "%E");
            ExtC39Translation.Add("!", "/A");
            ExtC39Translation.Add("\"", "/B");
            ExtC39Translation.Add("#", "/C");
            ExtC39Translation.Add("$", "/D");
            ExtC39Translation.Add("%", "/E");
            ExtC39Translation.Add("&", "/F");
            ExtC39Translation.Add("'", "/G");
            ExtC39Translation.Add("(", "/H");
            ExtC39Translation.Add(")", "/I");
            ExtC39Translation.Add("*", "/J");
            ExtC39Translation.Add("+", "/K");
            ExtC39Translation.Add(",", "/L");
            ExtC39Translation.Add("/", "/O");
            ExtC39Translation.Add(":", "/Z");
            ExtC39Translation.Add(";", "%F");
            ExtC39Translation.Add("<", "%G");
            ExtC39Translation.Add("=", "%H");
            ExtC39Translation.Add(">", "%I");
            ExtC39Translation.Add("?", "%J");
            ExtC39Translation.Add("[", "%K");
            ExtC39Translation.Add("\\", "%L");
            ExtC39Translation.Add("]", "%M");
            ExtC39Translation.Add("^", "%N");
            ExtC39Translation.Add("_", "%O");
            ExtC39Translation.Add("{", "%P");
            ExtC39Translation.Add("|", "%Q");
            ExtC39Translation.Add("}", "%R");
            ExtC39Translation.Add("~", "%S");
            ExtC39Translation.Add("`", "%W");
            ExtC39Translation.Add("@", "%V");
            ExtC39Translation.Add("a", "+A");
            ExtC39Translation.Add("b", "+B");
            ExtC39Translation.Add("c", "+C");
            ExtC39Translation.Add("d", "+D");
            ExtC39Translation.Add("e", "+E");
            ExtC39Translation.Add("f", "+F");
            ExtC39Translation.Add("g", "+G");
            ExtC39Translation.Add("h", "+H");
            ExtC39Translation.Add("i", "+I");
            ExtC39Translation.Add("j", "+J");
            ExtC39Translation.Add("k", "+K");
            ExtC39Translation.Add("l", "+L");
            ExtC39Translation.Add("m", "+M");
            ExtC39Translation.Add("n", "+N");
            ExtC39Translation.Add("o", "+O");
            ExtC39Translation.Add("p", "+P");
            ExtC39Translation.Add("q", "+Q");
            ExtC39Translation.Add("r", "+R");
            ExtC39Translation.Add("s", "+S");
            ExtC39Translation.Add("t", "+T");
            ExtC39Translation.Add("u", "+U");
            ExtC39Translation.Add("v", "+V");
            ExtC39Translation.Add("w", "+W");
            ExtC39Translation.Add("x", "+X");
            ExtC39Translation.Add("y", "+Y");
            ExtC39Translation.Add("z", "+Z");
            ExtC39Translation.Add(Convert.ToChar(127).ToString(), "%T"); // also %X, %Y, %Z 
        }
        private void InsertExtendedCharsIfNeeded(ref string formattedData)
        {
            var output = "";
            foreach (var c in formattedData)
            {
                try
                {
                    var s = C39Code[c].ToString();
                    output += c;
                }
                catch 
                { 
                    // insert extended substitution
                    var oTrans = ExtC39Translation[c.ToString()];
                    output += oTrans.ToString();
                }
            }

            formattedData = output;
        }
        private char GetChecksumChar(string strNoAstr) 
        {
            // checksum
            var Code39_Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%";
            var sum = 0;
            InsertExtendedCharsIfNeeded(ref strNoAstr);

            // Calculate the checksum
            foreach (var t in strNoAstr)
            {
                sum = sum + Code39_Charset.IndexOf(t.ToString(), StringComparison.Ordinal);
            }

            // return the checksum char
            return Code39_Charset[sum % 43];
        }

        #region IBarcode Members

        public string Encoded_Value => EncodeCode39();

        #endregion

    }
}
