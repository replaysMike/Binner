using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Binner.Common.Barcode
{
    /// <summary>
    /// Abstract Barcode Common class
    /// </summary>
    public abstract class BarcodeSymbology : IBarcode
    {
        /// <summary>
        /// The raw data that will be encoded to a barcode
        /// </summary>
        public string RawData { get; protected set; } = string.Empty;

        /// <summary>
        /// List of errors
        /// </summary>
        public List<string> Errors { get; protected set; } = new();

        public virtual string EncodedValue => throw new NotImplementedException();

        public void Error(string errorMessage)
        {
            Errors.Add(errorMessage);
            throw new Exception(errorMessage);
        }

        internal static bool CheckNumericOnly(string data) => Regex.IsMatch(data, @"^\d+$", RegexOptions.Compiled);
    }
}
