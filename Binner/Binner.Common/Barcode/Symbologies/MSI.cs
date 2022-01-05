using System;

namespace Binner.Common.Barcode.Symbologies
{
    public class MSI : BarcodeCommon, IBarcode
    {
        /// <summary>
        /// MSI encoding
        /// </summary>
        private readonly string[] MSI_Code = { "100100100100", "100100100110", "100100110100", "100100110110", "100110100100", "100110100110", "100110110100", "100110110110", "110100100100", "110100100110" };
        private readonly BarcodeType Encoded_Type = BarcodeType.Unspecified;

        public MSI(string input, BarcodeType encodedType)
        {
            Encoded_Type = encodedType;
            Raw_Data = input;
        }

        /// <summary>
        /// Encode the raw data using the MSI algorithm.
        /// </summary>
        private string EncodeMSI()
        {
            // check for non-numeric chars
            if (!CheckNumericOnly(Raw_Data))
                Error("EMSI-1: Numeric Data Only");

            var preEncoded = Raw_Data;

            // get checksum
            if (Encoded_Type == BarcodeType.MSI_Mod10 || Encoded_Type == BarcodeType.MSI_2Mod10)
            {
                var odds = "";
                var evens = "";
                for (var i = preEncoded.Length - 1; i >= 0; i -= 2)
                {
                    odds = preEncoded[i] + odds;
                    if (i - 1 >= 0)
                        evens = preEncoded[i - 1] + evens;
                }

                // multiply odds by 2
                odds = Convert.ToString((Int32.Parse(odds) * 2));

                var evensum = 0;
                var oddsum = 0;
                foreach (var c in evens)
                    evensum += Int32.Parse(c.ToString());
                foreach (var c in odds)
                    oddsum += Int32.Parse(c.ToString());
                var mod = (oddsum + evensum) % 10;
                var checksum = mod == 0 ? 0 : 10 - mod;
                preEncoded += checksum.ToString();
            }

            if (Encoded_Type == BarcodeType.MSI_Mod11 || Encoded_Type == BarcodeType.MSI_Mod11_Mod10)
            {
                var sum = 0;
                var weight = 2;
                for (var i = preEncoded.Length - 1; i >= 0; i--)
                {
                    if (weight > 7) weight = 2;
                    sum += Int32.Parse(preEncoded[i].ToString()) * weight++;
                }
                var mod = sum % 11;
                var checksum = mod == 0 ? 0 : 11 - mod;

                preEncoded += checksum.ToString();
            }

            if (Encoded_Type == BarcodeType.MSI_2Mod10 || Encoded_Type == BarcodeType.MSI_Mod11_Mod10)
            {
                // get second check digit if 2 mod 10 was selected or Mod11/Mod10
                var odds = "";
                var evens = "";
                for (var i = preEncoded.Length - 1; i >= 0; i -= 2)
                {
                    odds = preEncoded[i] + odds;
                    if (i - 1 >= 0)
                        evens = preEncoded[i - 1] + evens;
                }

                // multiply odds by 2
                odds = Convert.ToString((Int32.Parse(odds) * 2));

                var evensum = 0;
                var oddsum = 0;
                foreach (var c in evens)
                    evensum += Int32.Parse(c.ToString());
                foreach (var c in odds)
                    oddsum += Int32.Parse(c.ToString());
                var checksum = 10 - ((oddsum + evensum) % 10);
                preEncoded += checksum.ToString();
            }

            var result = "110";
            foreach (var c in preEncoded)
            {
                result += MSI_Code[Int32.Parse(c.ToString())];
            }

            // add stop character
            result += "1001";

            return result;
        }

        #region IBarcode Members

        public string Encoded_Value => EncodeMSI();

        #endregion

    }
}
