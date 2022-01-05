using System;

namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    ///  Blank encoding template
    ///  Written by: Brad Barnhill
    /// </summary>
    public class Blank : BarcodeSymbology, IBarcode
    {

        #region IBarcode Members

        public string EncodedValue
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
