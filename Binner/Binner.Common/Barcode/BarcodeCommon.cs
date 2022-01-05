using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Binner.Common.Barcode
{
    public abstract class BarcodeCommon
    {
        protected string Raw_Data = string.Empty;
        protected List<string> _Errors = new();

        public string RawData
        {
            get { return Raw_Data; }
        }

        public List<string> Errors
        {
            get { return _Errors; }
        }

        public void Error(string errorMessage)
        {
            _Errors.Add(errorMessage);
            throw new Exception(errorMessage);
        }

        internal static bool CheckNumericOnly(string data) => Regex.IsMatch(data, @"^\d+$", RegexOptions.Compiled);
    }
}
