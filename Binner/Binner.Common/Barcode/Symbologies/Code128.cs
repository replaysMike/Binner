using System;
using System.Collections.Generic;

namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// Code 128 encoding
    /// </summary>
    public class Code128 : BarcodeSymbology
    {
        public static readonly char FNC1 = Convert.ToChar(200);
        public static readonly char FNC2 = Convert.ToChar(201);
        public static readonly char FNC3 = Convert.ToChar(202);
        public static readonly char FNC4 = Convert.ToChar(203);

        public enum TYPES : int { DYNAMIC, A, B, C };
        private readonly List<string> _c128Code = new();
        private readonly Dictionary<string, int> _c128CodeIndexByA = new();
        private readonly Dictionary<string, int> _c128CodeIndexByB = new();
        private readonly Dictionary<string, int> _c128CodeIndexByC = new();
        private readonly List<string> _formattedData = new();
        private readonly List<string> _encodedData = new();
        private int? _startCharacterIndex;
        private readonly TYPES _type = TYPES.DYNAMIC;

        /// <summary>
        /// Encodes data in Code128 format.
        /// </summary>
        /// <param name="input">Data to encode.</param>
        public Code128(string input)
        {
            RawData = input;
        }

        /// <summary>
        /// Encodes data in Code128 format.
        /// </summary>
        /// <param name="input">Data to encode.</param>
        /// <param name="type">Type of encoding to lock to. (Code 128A, Code 128B, Code 128C)</param>
        public Code128(string input, TYPES type)
        {
            _type = type;
            RawData = input;
        }

        string _c128ByA(string a) => _c128Code[_c128CodeIndexByA[a]];
        string _c128TryByLookup(Dictionary<string, int> lookup, string value) => lookup.TryGetValue(value, out var index) ? _c128Code[index] : null;
        string _c128TryByA(string a) => _c128TryByLookup(_c128CodeIndexByA, a);
        string _c128TryByB(string b) => _c128TryByLookup(_c128CodeIndexByB, b);
        string _c128TryByC(string c) => _c128TryByLookup(_c128CodeIndexByC, c);

        private string Encode_Code128()
        {
            // initialize datastructure to hold encoding information
            InitCode128();

            return GetEncoding();
        }

        private void Clear()
        {
            _c128CodeIndexByA.Clear();
            _c128CodeIndexByB.Clear();
            _c128CodeIndexByC.Clear();
            _c128Code.Clear();
            _formattedData.Clear();
            _encodedData.Clear();
        }

        private void InitCode128()
        {
            Clear();

            // populate data
            AddEntry(" ", " ", "00", "11011001100");
            AddEntry("!", "!", "01", "11001101100");
            AddEntry("\"", "\"", "02", "11001100110");
            AddEntry("#", "#", "03", "10010011000");
            AddEntry("$", "$", "04", "10010001100");
            AddEntry("%", "%", "05", "10001001100");
            AddEntry("&", "&", "06", "10011001000");
            AddEntry("'", "'", "07", "10011000100");
            AddEntry("(", "(", "08", "10001100100");
            AddEntry(")", ")", "09", "11001001000");
            AddEntry("*", "*", "10", "11001000100");
            AddEntry("+", "+", "11", "11000100100");
            AddEntry(",", ",", "12", "10110011100");
            AddEntry("-", "-", "13", "10011011100");
            AddEntry(".", ".", "14", "10011001110");
            AddEntry("/", "/", "15", "10111001100");
            AddEntry("0", "0", "16", "10011101100");
            AddEntry("1", "1", "17", "10011100110");
            AddEntry("2", "2", "18", "11001110010");
            AddEntry("3", "3", "19", "11001011100");
            AddEntry("4", "4", "20", "11001001110");
            AddEntry("5", "5", "21", "11011100100");
            AddEntry("6", "6", "22", "11001110100");
            AddEntry("7", "7", "23", "11101101110");
            AddEntry("8", "8", "24", "11101001100");
            AddEntry("9", "9", "25", "11100101100");
            AddEntry(":", ":", "26", "11100100110");
            AddEntry(";", ";", "27", "11101100100");
            AddEntry("<", "<", "28", "11100110100");
            AddEntry("=", "=", "29", "11100110010");
            AddEntry(">", ">", "30", "11011011000");
            AddEntry("?", "?", "31", "11011000110");
            AddEntry("@", "@", "32", "11000110110");
            AddEntry("A", "A", "33", "10100011000");
            AddEntry("B", "B", "34", "10001011000");
            AddEntry("C", "C", "35", "10001000110");
            AddEntry("D", "D", "36", "10110001000");
            AddEntry("E", "E", "37", "10001101000");
            AddEntry("F", "F", "38", "10001100010");
            AddEntry("G", "G", "39", "11010001000");
            AddEntry("H", "H", "40", "11000101000");
            AddEntry("I", "I", "41", "11000100010");
            AddEntry("J", "J", "42", "10110111000");
            AddEntry("K", "K", "43", "10110001110");
            AddEntry("L", "L", "44", "10001101110");
            AddEntry("M", "M", "45", "10111011000");
            AddEntry("N", "N", "46", "10111000110");
            AddEntry("O", "O", "47", "10001110110");
            AddEntry("P", "P", "48", "11101110110");
            AddEntry("Q", "Q", "49", "11010001110");
            AddEntry("R", "R", "50", "11000101110");
            AddEntry("S", "S", "51", "11011101000");
            AddEntry("T", "T", "52", "11011100010");
            AddEntry("U", "U", "53", "11011101110");
            AddEntry("V", "V", "54", "11101011000");
            AddEntry("W", "W", "55", "11101000110");
            AddEntry("X", "X", "56", "11100010110");
            AddEntry("Y", "Y", "57", "11101101000");
            AddEntry("Z", "Z", "58", "11101100010");
            AddEntry("[", "[", "59", "11100011010");
            AddEntry(@"\", @"\", "60", "11101111010");
            AddEntry("]", "]", "61", "11001000010");
            AddEntry("^", "^", "62", "11110001010");
            AddEntry("_", "_", "63", "10100110000");
            AddEntry("\0", "`", "64", "10100001100");
            AddEntry(Convert.ToChar(1).ToString(), "a", "65", "10010110000");
            AddEntry(Convert.ToChar(2).ToString(), "b", "66", "10010000110");
            AddEntry(Convert.ToChar(3).ToString(), "c", "67", "10000101100");
            AddEntry(Convert.ToChar(4).ToString(), "d", "68", "10000100110");
            AddEntry(Convert.ToChar(5).ToString(), "e", "69", "10110010000");
            AddEntry(Convert.ToChar(6).ToString(), "f", "70", "10110000100");
            AddEntry(Convert.ToChar(7).ToString(), "g", "71", "10011010000");
            AddEntry(Convert.ToChar(8).ToString(), "h", "72", "10011000010");
            AddEntry(Convert.ToChar(9).ToString(), "i", "73", "10000110100");
            AddEntry(Convert.ToChar(10).ToString(), "j", "74", "10000110010");
            AddEntry(Convert.ToChar(11).ToString(), "k", "75", "11000010010");
            AddEntry(Convert.ToChar(12).ToString(), "l", "76", "11001010000");
            AddEntry(Convert.ToChar(13).ToString(), "m", "77", "11110111010");
            AddEntry(Convert.ToChar(14).ToString(), "n", "78", "11000010100");
            AddEntry(Convert.ToChar(15).ToString(), "o", "79", "10001111010");
            AddEntry(Convert.ToChar(16).ToString(), "p", "80", "10100111100");
            AddEntry(Convert.ToChar(17).ToString(), "q", "81", "10010111100");
            AddEntry(Convert.ToChar(18).ToString(), "r", "82", "10010011110");
            AddEntry(Convert.ToChar(19).ToString(), "s", "83", "10111100100");
            AddEntry(Convert.ToChar(20).ToString(), "t", "84", "10011110100");
            AddEntry(Convert.ToChar(21).ToString(), "u", "85", "10011110010");
            AddEntry(Convert.ToChar(22).ToString(), "v", "86", "11110100100");
            AddEntry(Convert.ToChar(23).ToString(), "w", "87", "11110010100");
            AddEntry(Convert.ToChar(24).ToString(), "x", "88", "11110010010");
            AddEntry(Convert.ToChar(25).ToString(), "y", "89", "11011011110");
            AddEntry(Convert.ToChar(26).ToString(), "z", "90", "11011110110");
            AddEntry(Convert.ToChar(27).ToString(), "{", "91", "11110110110");
            AddEntry(Convert.ToChar(28).ToString(), "|", "92", "10101111000");
            AddEntry(Convert.ToChar(29).ToString(), "}", "93", "10100011110");
            AddEntry(Convert.ToChar(30).ToString(), "~", "94", "10001011110");

            AddEntry(Convert.ToChar(31).ToString(), Convert.ToChar(127).ToString(), "95", "10111101000");
            AddEntry("FNC3", "FNC3", "96", "10111100010");
            AddEntry("FNC2", "FNC2", "97", "11110101000");
            AddEntry("SHIFT", "SHIFT", "98", "11110100010");
            AddEntry("CODE_C", "CODE_C", "99", "10111011110");
            AddEntry("CODE_B", "FNC4", "CODE_B", "10111101110");
            AddEntry("FNC4", "CODE_A", "CODE_A", "11101011110");
            AddEntry("FNC1", "FNC1", "FNC1", "11110101110");
            AddEntry("START_A", "START_A", "START_A", "11010000100");
            AddEntry("START_B", "START_B", "START_B", "11010010000");
            AddEntry("START_C", "START_C", "START_C", "11010011100");
            AddEntry("STOP", "STOP", "STOP", "11000111010");

            void AddEntry(string a, string b, string c, string encoding)
            {
                _c128CodeIndexByA.Add(a, _c128Code.Count);
                _c128CodeIndexByB.Add(b, _c128Code.Count);
                _c128CodeIndexByC.Add(c, _c128Code.Count);
                _c128Code.Add(encoding);
            }
        }
        private List<int> FindStartorCodeCharacter(string s)
        {
            var rows = new List<int>();

            // if two chars are numbers (or FNC1) then START_C or CODE_C
            if (s.Length > 1 && (char.IsNumber(s[0]) || s[0] == FNC1) && (char.IsNumber(s[1]) || s[1] == FNC1))
            {
                if (!_startCharacterIndex.HasValue)
                {
                    _startCharacterIndex = _c128CodeIndexByA["START_C"];
                    rows.Add(_startCharacterIndex.Value);
                }
                else
                    rows.Add(_c128CodeIndexByA["CODE_C"]);
            }
            else
            {
                try
                {
                    var aFound = _c128CodeIndexByA.TryGetValue(s, out var aIndex);
                    if (aFound)
                    {
                        if (!_startCharacterIndex.HasValue)
                        {
                            _startCharacterIndex = _c128CodeIndexByA["START_A"];
                            rows.Add(_startCharacterIndex.Value);
                        }
                        else
                        {
                            rows.Add(_c128CodeIndexByB["CODE_A"]);//first column is FNC4 so use B
                        }
                    }
                    var bFound = _c128CodeIndexByB.TryGetValue(s, out var bIndex) && (!aFound || bIndex != aIndex);
                    if (bFound)
                    {
                        if (!_startCharacterIndex.HasValue)
                        {
                            _startCharacterIndex = _c128CodeIndexByA["START_B"];
                            rows.Add(_startCharacterIndex.Value);
                        }
                        else
                        {
                            rows.Add(_c128CodeIndexByA["CODE_B"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error("EC128-1: " + ex.Message);
                }

                if (rows.Count <= 0)
                    Error("EC128-2: Could not determine start character.");
            }

            return rows;
        }

        private string CalculateCheckDigit()
        {
            uint checkSum = 0;

            for (uint i = 0; i < _formattedData.Count; i++)
            {
                var s = _formattedData[(int)i];

                // try to find value in the A column
                var value = _c128CodeIndexByA.TryGetValue(s, out var index)
                // try to find value in the B column
                    || _c128CodeIndexByB.TryGetValue(s, out index)
                // try to find value in the C column
                    || _c128CodeIndexByC.TryGetValue(s, out index) ? (uint)index : throw new InvalidOperationException($"Unable to find character “{s}”");

                var addition = value * ((i == 0) ? 1 : i);
                checkSum += addition;
            }

            var remainder = checkSum % 103;
            return _c128Code[(int)remainder];
        }

        private void BreakUpDataForEncoding()
        {
            var temp = "";
            var tempRawData = RawData;

            // breaking the raw data up for code A and code B will mess up the encoding
            switch (_type)
            {
                case TYPES.A:
                case TYPES.B:
                    {
                        foreach (var c in RawData)
                            _formattedData.Add(c.ToString());
                        return;
                    }

                case TYPES.C:
                    {
                        var indexOfFirstNumeric = -1;
                        var numericCount = 0;
                        for (var x = 0; x < RawData.Length; x++)
                        {
                            var c = RawData[x];
                            if (char.IsNumber(c))
                            {
                                numericCount++;
                                if (indexOfFirstNumeric == -1)
                                {
                                    indexOfFirstNumeric = x;
                                }
                            }
                            else if (c != FNC1)
                            {
                                Error("EC128-6: Only numeric values can be encoded with C128-C (Invalid char at position " + x + ").");
                            }
                        }

                        // CODE C: adds a 0 to the front of the Raw_Data if the length is not divisible by 2
                        if (numericCount % 2 == 1)
                            tempRawData = tempRawData.Insert(indexOfFirstNumeric, "0");
                        break;
                    }
            }

            foreach (var c in tempRawData)
            {
                if (char.IsNumber(c))
                {
                    if (temp == "")
                    {
                        temp += c;
                    }
                    else
                    {
                        temp += c;
                        _formattedData.Add(temp);
                        temp = "";
                    }
                }
                else
                {
                    if (temp != "")
                    {
                        _formattedData.Add(temp);
                        temp = "";
                    }
                    _formattedData.Add(c.ToString());
                }
            }

            // if something is still in temp go ahead and push it onto the queue
            if (temp != "")
            {
                _formattedData.Add(temp);
            }
        }
        private void InsertStartandCodeCharacters()
        {
            if (_type != TYPES.DYNAMIC)
            {
                switch (_type)
                {
                    case TYPES.A:
                        _formattedData.Insert(0, "START_A");
                        break;
                    case TYPES.B:
                        _formattedData.Insert(0, "START_B");
                        break;
                    case TYPES.C:
                        _formattedData.Insert(0, "START_C");
                        break;
                    default:
                        Error("EC128-4: Unknown start type in fixed type encoding.");
                        break;
                }
            }
            else
            {
                try
                {
                    Dictionary<string, int> col = null;

                    for (var i = 0; i < _formattedData.Count; i++)
                    {
                        var currentElement = _formattedData[i];
                        var tempStartChars = FindStartorCodeCharacter(currentElement);

                        // check all the start characters and see if we need to stay with the same codeset or if a change of sets is required
                        var sameCodeSet = false;
                        foreach (var row in tempStartChars)
                        {
                            if (_c128CodeIndexByA.TryGetValue(currentElement, out var index) && index == row
                                || _c128CodeIndexByB.TryGetValue(currentElement, out index) && index == row
                                || _c128CodeIndexByC.TryGetValue(currentElement, out index) && index == row)
                            {
                                sameCodeSet = true;
                                break;
                            }
                        }

                        // only insert a new code char if starting a new codeset
                        // if (CurrentCodeString == "" || !tempStartChars[0][col].ToString().EndsWith(CurrentCodeString)) /* Removed because of bug */

                        if (col == null || !sameCodeSet)
                        {
                            var currentCodeSet = tempStartChars[0];

                            foreach (var (start, code, nextCol) in new[] {
                                ("START_A", "CODE_A", _c128CodeIndexByA),
                                ("START_B", "CODE_B", _c128CodeIndexByB),
                                ("START_C", "CODE_C", _c128CodeIndexByC),
                            })
                            {
                                if (col == null)
                                {
                                    // We still need to write the start char and establish the current code.
                                    if (currentCodeSet == _c128CodeIndexByA[start])
                                    {
                                        col = nextCol;
                                        _formattedData.Insert(i++, start);
                                        break;
                                    }
                                }
                                else
                                {
                                    // We need to switch codes.
                                    if (col != nextCol && currentCodeSet == col[code])
                                    {
                                        col = nextCol;
                                        _formattedData.Insert(i++, code);
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Error("EC128-3: Could not insert start and code characters.\n Message: " + ex.Message);
                }
            }
        }

        private string GetEncoding()
        {
            // break up data for encoding
            BreakUpDataForEncoding();

            // insert the start characters
            InsertStartandCodeCharacters();

            var encodedData = "";
            foreach (var s in _formattedData)
            {
                //handle exception with apostrophes in select statements
                string eRow;

                //select encoding only for type selected
                switch (_type)
                {
                    case TYPES.A:
                        eRow = _c128TryByA(s);
                        break;
                    case TYPES.B:
                        eRow = _c128TryByB(s);
                        break;
                    case TYPES.C:
                        eRow = _c128TryByC(s);
                        break;
                    case TYPES.DYNAMIC:
                        eRow = _c128TryByA(s);

                        if (eRow == null)
                        {
                            eRow = _c128TryByB(s);

                            if (eRow == null)
                            {
                                eRow = _c128TryByC(s);
                            }
                        }
                        break;
                    default:
                        eRow = null;
                        break;
                }

                if (eRow == null)
                    Error("EC128-5: Could not find encoding of a value( " + s + " ) in C128 type " + _type.ToString());

                encodedData += eRow;
                _encodedData.Add(eRow);
            }

            // add the check digit
            var checkDigit = CalculateCheckDigit();
            encodedData += checkDigit;
            _encodedData.Add(checkDigit);

            // add the stop character
            var stop = _c128ByA("STOP");
            encodedData += stop;
            _encodedData.Add(stop);

            // add the termination bars
            encodedData += "11";
            _encodedData.Add("11");

            return encodedData;
        }

        #region IBarcode Members

        public override string EncodedValue => Encode_Code128();

        #endregion

    }
}
