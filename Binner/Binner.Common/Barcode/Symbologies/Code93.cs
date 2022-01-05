using System;

namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// Code 93 encoding
    /// </summary>
    public class Code93 : BarcodeSymbology, IBarcode
    {
        private readonly System.Data.DataTable C93Code = new System.Data.DataTable("C93_Code");

        /// <summary>
        /// Encodes with Code93.
        /// </summary>
        /// <param name="input">Data to encode.</param>
        public Code93(string input)
        {
            RawData = input;
        }

        /// <summary>
        /// Encode the raw data using the Code 93 algorithm.
        /// </summary>
        private string EncodeCode93()
        {
            InitCode93();

            var formattedData = AddCheckDigits(RawData);

            var result = C93Code.Select("Character = '*'")[0]["Encoding"].ToString();
            foreach (var c in formattedData)
            {
                try
                {
                    result += C93Code.Select("Character = '" + c + "'")[0]["Encoding"].ToString();
                }
                catch
                {
                        Error("EC93-1: Invalid data.");
                }
            }

            result += C93Code.Select("Character = '*'")[0]["Encoding"].ToString();

            //termination bar
            result += "1";

            //clear the hashtable so it no longer takes up memory
            C93Code.Clear();

            return result;
        }
        private void InitCode93()
        {
            C93Code.Rows.Clear();
            C93Code.Columns.Clear();
            C93Code.Columns.Add("Value");
            C93Code.Columns.Add("Character");
            C93Code.Columns.Add("Encoding");
            C93Code.Rows.Add(new object[] { "0", "0", "100010100" });
            C93Code.Rows.Add(new object[] { "1", "1", "101001000" });
            C93Code.Rows.Add(new object[] { "2", "2", "101000100" });
            C93Code.Rows.Add(new object[] { "3", "3", "101000010" });
            C93Code.Rows.Add(new object[] { "4",  "4", "100101000" });
            C93Code.Rows.Add(new object[] { "5",  "5", "100100100" });
            C93Code.Rows.Add(new object[] { "6",  "6", "100100010" });
            C93Code.Rows.Add(new object[] { "7",  "7", "101010000" });
            C93Code.Rows.Add(new object[] { "8",  "8", "100010010" });
            C93Code.Rows.Add(new object[] { "9",  "9", "100001010" });
            C93Code.Rows.Add(new object[] { "10", "A", "110101000" });
            C93Code.Rows.Add(new object[] { "11", "B", "110100100" });
            C93Code.Rows.Add(new object[] { "12", "C", "110100010" });
            C93Code.Rows.Add(new object[] { "13", "D", "110010100" });
            C93Code.Rows.Add(new object[] { "14", "E", "110010010" });
            C93Code.Rows.Add(new object[] { "15", "F", "110001010" });
            C93Code.Rows.Add(new object[] { "16", "G", "101101000" });
            C93Code.Rows.Add(new object[] { "17", "H", "101100100" });
            C93Code.Rows.Add(new object[] { "18", "I", "101100010" });
            C93Code.Rows.Add(new object[] { "19", "J", "100110100" });
            C93Code.Rows.Add(new object[] { "20", "K", "100011010" });
            C93Code.Rows.Add(new object[] { "21", "L", "101011000" });
            C93Code.Rows.Add(new object[] { "22", "M", "101001100" });
            C93Code.Rows.Add(new object[] { "23", "N", "101000110" });
            C93Code.Rows.Add(new object[] { "24", "O", "100101100" });
            C93Code.Rows.Add(new object[] { "25", "P", "100010110" });
            C93Code.Rows.Add(new object[] { "26", "Q", "110110100" });
            C93Code.Rows.Add(new object[] { "27", "R", "110110010" });
            C93Code.Rows.Add(new object[] { "28", "S", "110101100" });
            C93Code.Rows.Add(new object[] { "29", "T", "110100110" });
            C93Code.Rows.Add(new object[] { "30", "U", "110010110" });
            C93Code.Rows.Add(new object[] { "31", "V", "110011010" });
            C93Code.Rows.Add(new object[] { "32", "W", "101101100" });
            C93Code.Rows.Add(new object[] { "33", "X", "101100110" });
            C93Code.Rows.Add(new object[] { "34", "Y", "100110110" });
            C93Code.Rows.Add(new object[] { "35", "Z", "100111010" });
            C93Code.Rows.Add(new object[] { "36", "-", "100101110" });
            C93Code.Rows.Add(new object[] { "37", ".", "111010100" });
            C93Code.Rows.Add(new object[] { "38", " ", "111010010" });
            C93Code.Rows.Add(new object[] { "39", "$", "111001010" });
            C93Code.Rows.Add(new object[] { "40", "/", "101101110" });
            C93Code.Rows.Add(new object[] { "41", "+", "101110110" });
            C93Code.Rows.Add(new object[] { "42", "%", "110101110" });
            C93Code.Rows.Add(new object[] { "43", "(", "100100110" });//dont know what character actually goes here
            C93Code.Rows.Add(new object[] { "44", ")", "111011010" });//dont know what character actually goes here
            C93Code.Rows.Add(new object[] { "45", "#", "111010110" });//dont know what character actually goes here
            C93Code.Rows.Add(new object[] { "46", "@", "100110010" });//dont know what character actually goes here
            C93Code.Rows.Add(new object[] { "-",  "*", "101011110" });
        }

        private string AddCheckDigits(string input)
        {
            // populate the C weights
            var aryCWeights = new int[input.Length];
            var curweight = 1;
            for (var i = input.Length - 1; i >= 0; i--)
            {
                if (curweight > 20)
                    curweight = 1;
                aryCWeights[i] = curweight;
                curweight++;
            }

            // populate the K weights
            var aryKWeights = new int[input.Length + 1];
            curweight = 1;
            for (var i = input.Length; i >= 0; i--)
            {
                if (curweight > 15)
                    curweight = 1;
                aryKWeights[i] = curweight;
                curweight++;
            }

            //calculate C checksum
            var sum = 0;
            for (var i = 0; i < input.Length; i++)
            {
                sum += aryCWeights[i] * int.Parse(C93Code.Select("Character = '" + input[i] + "'")[0]["Value"].ToString());
            }
            var checksumValue = sum % 47;

            input += C93Code.Select("Value = '" + checksumValue + "'")[0]["Character"].ToString();

            //calculate K checksum
            sum = 0;
            for (var i = 0; i < input.Length; i++)
            {
                sum += aryKWeights[i] * int.Parse(C93Code.Select("Character = '" + input[i] + "'")[0]["Value"].ToString());
            }
            checksumValue = sum % 47;

            input += C93Code.Select("Value = '" + checksumValue + "'")[0]["Character"].ToString();

            return input;
        }
        
        #region IBarcode Members

        public string Encoded_Value => EncodeCode93();

        #endregion

    }
}

