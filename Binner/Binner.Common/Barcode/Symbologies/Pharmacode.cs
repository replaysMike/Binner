using System;

namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    /// Pharmacode encoding
    /// </summary>
    public class Pharmacode: BarcodeCommon, IBarcode
    {
        private readonly string _thinBar = "1";
        private readonly string _gap = "00";
        private readonly string _thickBar = "111";

        /// <summary>
        /// Encodes with Pharmacode.
        /// </summary>
        /// <param name="input">Data to encode.</param>
        public Pharmacode(string input)
        {
            Raw_Data = input;

            if (!CheckNumericOnly(Raw_Data))
            {
                Error("EPHARM-1: Data contains invalid  characters (non-numeric).");
            }
            else if (Raw_Data.Length > 6)
            {
                Error("EPHARM-2: Data too long (invalid data input length).");
            }
        }

        /// <summary>
        /// Encode the raw data using the Pharmacode algorithm.
        /// </summary>
        private string EncodePharmacode()
        {

            if (!Int32.TryParse(Raw_Data, out int num))
            {
                Error("EPHARM-3: Input is unparseable.");
            }
            else if (num < 3 || num > 131070)
            {
                Error("EPHARM-4: Data contains invalid  characters (invalid numeric range).");
            }

            var result = String.Empty;
            do
            {
                if ((num & 1) == 0)
                {
                    result = _thickBar + result;
                    num = (num - 2) / 2;
                }
                else
                {
                    result = _thinBar + result;
                    num = (num - 1) / 2;
                }

                if (num != 0)
                {
                    result = _gap + result;
                }
            } while (num != 0);

            return result;
        }

        #region IBarcode Members

        public string Encoded_Value => EncodePharmacode();

        #endregion

    }
}
