using System.Collections.Generic;

namespace Binner.Common.Barcode
{
    /// <summary>
    ///  Barcode interface for symbology layout.
    /// </summary>
    interface IBarcode
    {
        string EncodedValue { get; }

        string RawData { get; }

        List<string> Errors { get; }
    }
}
