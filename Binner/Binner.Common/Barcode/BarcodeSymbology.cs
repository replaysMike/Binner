using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Binner.Common.Barcode
{
    /// <summary>
    /// Abstract Barcode Common class
    /// </summary>
    public abstract class BarcodeSymbology
    {
        public string RawData { get; protected set; } = string.Empty;

        public List<string> Errors { get; protected set; } = new();

        public void Error(string errorMessage)
        {
            Errors.Add(errorMessage);
            throw new Exception(errorMessage);
        }

        internal static bool CheckNumericOnly(string data) => Regex.IsMatch(data, @"^\d+$", RegexOptions.Compiled);
    }
}
