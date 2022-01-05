using System;
using System.Collections;

namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// Telepen encoding
    /// </summary>
    public class Telepen : BarcodeSymbology
    {
        private static readonly Hashtable _telepenCode = new();
        private enum StartStopCode : int { Start1, Stop1, Start2, Stop2, Start3, Stop3 };
        private StartStopCode _startCode = StartStopCode.Start1;
        private StartStopCode _stopCode = StartStopCode.Stop1;
        private int _switchModeIndex;
        private int _iCheckSum;

        /// <summary>
        /// Encodes data using the Telepen algorithm.
        /// </summary>
        /// <param name="input"></param>
        public Telepen(string input)
        {
            RawData = input;
        }

        /// <summary>
        /// Encode the raw data using the Telepen algorithm.
        /// </summary>
        private string EncodeTelepen()
        {
            InitTelepen();

            _iCheckSum = 0;
            SetEncodingSequence();

            // include the Start sequence pattern
            var result = _telepenCode[_startCode].ToString();
            switch (_startCode)
            {
                // numeric --> ascii
                case StartStopCode.Start2:
                    EncodeNumeric(RawData.Substring(0, _switchModeIndex), ref result);

                    if (_switchModeIndex < RawData.Length)
                    {
                        EncodeSwitchMode(ref result);
                        EncodeASCII(RawData.Substring(_switchModeIndex), ref result);
                    }
                    break;
                // ascii --> numeric
                case StartStopCode.Start3:
                    EncodeASCII(RawData.Substring(0, _switchModeIndex), ref result);
                    EncodeSwitchMode(ref result);
                    EncodeNumeric(RawData.Substring(_switchModeIndex), ref result);
                    break;
                // full ascii
                default:
                    EncodeASCII(RawData, ref result);
                    break;
            }

            // checksum
            result += _telepenCode[CalculateChecksum(_iCheckSum)];

            // stop character
            result += _telepenCode[_stopCode];

            return result;
        }

        private void EncodeASCII(string input, ref string output)
        {
            try
            {
                foreach (var c in input)
                {
                    output += _telepenCode[c];
                    _iCheckSum += Convert.ToInt32(c);
                }
            }
            catch
            {
                Error("ETELEPEN-1: Invalid data when encoding ASCII");
            }
        }
        private void EncodeNumeric(string input, ref string output)
        {
            try
            {
                if ((input.Length % 2) > 0)
                    Error("ETELEPEN-3: Numeric encoding attempted on odd number of characters");

                for (var i = 0; i < input.Length; i += 2)
                {
                    output += _telepenCode[Convert.ToChar(int.Parse(input.Substring(i, 2)) + 27)];
                    _iCheckSum += int.Parse(input.Substring(i, 2)) + 27;
                }
            }
            catch
            {
                Error("ETELEPEN-2: Numeric encoding failed");
            }
        }
        private void EncodeSwitchMode(ref string output)
        {
            // ASCII code DLE is used to switch modes
            _iCheckSum += 16;
            output += _telepenCode[Convert.ToChar(16)];
        }

        private char CalculateChecksum(int iCheckSum) => Convert.ToChar(127 - (iCheckSum % 127));

        private void SetEncodingSequence()
        {
            // reset to full ascii
            _startCode = StartStopCode.Start1;
            _stopCode = StartStopCode.Stop1;
            _switchModeIndex = RawData.Length;

            // starting number of 'numbers'
            var startNumerics = 0;
            foreach (var c in RawData)
            {
                if (char.IsNumber(c))
                    startNumerics++;
                else
                    break;
            }

            if (startNumerics == RawData.Length)
            {
                // Numeric only mode due to only numbers being present
                _startCode = StartStopCode.Start2;
                _stopCode = StartStopCode.Stop2;

                if ((RawData.Length % 2) > 0)
                    _switchModeIndex = RawData.Length - 1;
            }
            else
            {
                // ending number of numbers
                var endNumerics = 0;
                for (var i = RawData.Length - 1; i >= 0; i--)
                {
                    if (char.IsNumber(RawData[i]))
                        endNumerics++;
                    else
                        break;
                }

                if (startNumerics >= 4 || endNumerics >= 4)
                {
                    // hybrid mode will be used
                    if (startNumerics > endNumerics)
                    {
                        // start in numeric switching to ascii
                        _startCode = StartStopCode.Start2;
                        _stopCode = StartStopCode.Stop2;
                        _switchModeIndex = (startNumerics % 2) == 1 ? startNumerics - 1 : startNumerics;
                    }
                    else
                    {
                        // start in ascii switching to numeric
                        _startCode = StartStopCode.Start3;
                        _stopCode = StartStopCode.Stop3;
                        _switchModeIndex = (endNumerics % 2) == 1 ? RawData.Length - endNumerics + 1 : RawData.Length - endNumerics;
                    }
                }
            }
        }

        private void InitTelepen()
        {
            _telepenCode.Clear();
            _telepenCode.Add(Convert.ToChar(0), "1110111011101110");
            _telepenCode.Add(Convert.ToChar(1), "1011101110111010");
            _telepenCode.Add(Convert.ToChar(2), "1110001110111010");
            _telepenCode.Add(Convert.ToChar(3), "1010111011101110");
            _telepenCode.Add(Convert.ToChar(4), "1110101110111010");
            _telepenCode.Add(Convert.ToChar(5), "1011100011101110");
            _telepenCode.Add(Convert.ToChar(6), "1000100011101110");
            _telepenCode.Add(Convert.ToChar(7), "1010101110111010");
            _telepenCode.Add(Convert.ToChar(8), "1110111000111010");
            _telepenCode.Add(Convert.ToChar(9), "1011101011101110");
            _telepenCode.Add(Convert.ToChar(10), "1110001011101110");
            _telepenCode.Add(Convert.ToChar(11), "1010111000111010");
            _telepenCode.Add(Convert.ToChar(12), "1110101011101110");
            _telepenCode.Add(Convert.ToChar(13), "1010001000111010");
            _telepenCode.Add(Convert.ToChar(14), "1000101000111010");
            _telepenCode.Add(Convert.ToChar(15), "1010101011101110");
            _telepenCode.Add(Convert.ToChar(16), "1110111010111010");
            _telepenCode.Add(Convert.ToChar(17), "1011101110001110");
            _telepenCode.Add(Convert.ToChar(18), "1110001110001110");
            _telepenCode.Add(Convert.ToChar(19), "1010111010111010");
            _telepenCode.Add(Convert.ToChar(20), "1110101110001110");
            _telepenCode.Add(Convert.ToChar(21), "1011100010111010");
            _telepenCode.Add(Convert.ToChar(22), "1000100010111010");
            _telepenCode.Add(Convert.ToChar(23), "1010101110001110");
            _telepenCode.Add(Convert.ToChar(24), "1110100010001110");
            _telepenCode.Add(Convert.ToChar(25), "1011101010111010");
            _telepenCode.Add(Convert.ToChar(26), "1110001010111010");
            _telepenCode.Add(Convert.ToChar(27), "1010100010001110");
            _telepenCode.Add(Convert.ToChar(28), "1110101010111010");
            _telepenCode.Add(Convert.ToChar(29), "1010001010001110");
            _telepenCode.Add(Convert.ToChar(30), "1000101010001110");
            _telepenCode.Add(Convert.ToChar(31), "1010101010111010");
            _telepenCode.Add(' ', "1110111011100010");
            _telepenCode.Add('!', "1011101110101110");
            _telepenCode.Add('"', "1110001110101110");
            _telepenCode.Add('#', "1010111011100010");
            _telepenCode.Add('$', "1110101110101110");
            _telepenCode.Add('%', "1011100011100010");
            _telepenCode.Add('&', "1000100011100010");
            _telepenCode.Add('\'', "1010101110101110");
            _telepenCode.Add('(', "1110111000101110");
            _telepenCode.Add(')', "1011101011100010");
            _telepenCode.Add('*', "1110001011100010");
            _telepenCode.Add('+', "1010111000101110");
            _telepenCode.Add(',', "1110101011100010");
            _telepenCode.Add('-', "1010001000101110");
            _telepenCode.Add('.', "1000101000101110");
            _telepenCode.Add('/', "1010101011100010");
            _telepenCode.Add('0', "1110111010101110");
            _telepenCode.Add('1', "1011101000100010");
            _telepenCode.Add('2', "1110001000100010");
            _telepenCode.Add('3', "1010111010101110");
            _telepenCode.Add('4', "1110101000100010");
            _telepenCode.Add('5', "1011100010101110");
            _telepenCode.Add('6', "1000100010101110");
            _telepenCode.Add('7', "1010101000100010");
            _telepenCode.Add('8', "1110100010100010");
            _telepenCode.Add('9', "1011101010101110");
            _telepenCode.Add(':', "1110001010101110");
            _telepenCode.Add(';', "1010100010100010");
            _telepenCode.Add('<', "1110101010101110");
            _telepenCode.Add('=', "1010001010100010");
            _telepenCode.Add('>', "1000101010100010");
            _telepenCode.Add('?', "1010101010101110");
            _telepenCode.Add('@', "1110111011101010");
            _telepenCode.Add('A', "1011101110111000");
            _telepenCode.Add('B', "1110001110111000");
            _telepenCode.Add('C', "1010111011101010");
            _telepenCode.Add('D', "1110101110111000");
            _telepenCode.Add('E', "1011100011101010");
            _telepenCode.Add('F', "1000100011101010");
            _telepenCode.Add('G', "1010101110111000");
            _telepenCode.Add('H', "1110111000111000");
            _telepenCode.Add('I', "1011101011101010");
            _telepenCode.Add('J', "1110001011101010");
            _telepenCode.Add('K', "1010111000111000");
            _telepenCode.Add('L', "1110101011101010");
            _telepenCode.Add('M', "1010001000111000");
            _telepenCode.Add('N', "1000101000111000");
            _telepenCode.Add('O', "1010101011101010");
            _telepenCode.Add('P', "1110111010111000");
            _telepenCode.Add('Q', "1011101110001010");
            _telepenCode.Add('R', "1110001110001010");
            _telepenCode.Add('S', "1010111010111000");
            _telepenCode.Add('T', "1110101110001010");
            _telepenCode.Add('U', "1011100010111000");
            _telepenCode.Add('V', "1000100010111000");
            _telepenCode.Add('W', "1010101110001010");
            _telepenCode.Add('X', "1110100010001010");
            _telepenCode.Add('Y', "1011101010111000");
            _telepenCode.Add('Z', "1110001010111000");
            _telepenCode.Add('[', "1010100010001010");
            _telepenCode.Add('\\', "1110101010111000");
            _telepenCode.Add(']', "1010001010001010");
            _telepenCode.Add('^', "1000101010001010");
            _telepenCode.Add('_', "1010101010111000");
            _telepenCode.Add('`', "1110111010001000");
            _telepenCode.Add('a', "1011101110101010");
            _telepenCode.Add('b', "1110001110101010");
            _telepenCode.Add('c', "1010111010001000");
            _telepenCode.Add('d', "1110101110101010");
            _telepenCode.Add('e', "1011100010001000");
            _telepenCode.Add('f', "1000100010001000");
            _telepenCode.Add('g', "1010101110101010");
            _telepenCode.Add('h', "1110111000101010");
            _telepenCode.Add('i', "1011101010001000");
            _telepenCode.Add('j', "1110001010001000");
            _telepenCode.Add('k', "1010111000101010");
            _telepenCode.Add('l', "1110101010001000");
            _telepenCode.Add('m', "1010001000101010");
            _telepenCode.Add('n', "1000101000101010");
            _telepenCode.Add('o', "1010101010001000");
            _telepenCode.Add('p', "1110111010101010");
            _telepenCode.Add('q', "1011101000101000");
            _telepenCode.Add('r', "1110001000101000");
            _telepenCode.Add('s', "1010111010101010");
            _telepenCode.Add('t', "1110101000101000");
            _telepenCode.Add('u', "1011100010101010");
            _telepenCode.Add('v', "1000100010101010");
            _telepenCode.Add('w', "1010101000101000");
            _telepenCode.Add('x', "1110100010101000");
            _telepenCode.Add('y', "1011101010101010");
            _telepenCode.Add('z', "1110001010101010");
            _telepenCode.Add('{', "1010100010101000");
            _telepenCode.Add('|', "1110101010101010");
            _telepenCode.Add('}', "1010001010101000");
            _telepenCode.Add('~', "1000101010101000");
            _telepenCode.Add(Convert.ToChar(127), "1010101010101010");
            _telepenCode.Add(StartStopCode.Start1, "1010101010111000");
            _telepenCode.Add(StartStopCode.Stop1, "1110001010101010");
            _telepenCode.Add(StartStopCode.Start2, "1010101011101000");
            _telepenCode.Add(StartStopCode.Stop2, "1110100010101010");
            _telepenCode.Add(StartStopCode.Start3, "1010101110101000");
            _telepenCode.Add(StartStopCode.Stop3, "1110101000101010");
        }

        #region IBarcode Members

        public override string EncodedValue => EncodeTelepen();

        #endregion

    }
}
