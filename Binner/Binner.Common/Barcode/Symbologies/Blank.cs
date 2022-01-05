using System;

namespace Binner.Common.Barcode.Symbologies
{
    /// <summary>
    ///  Blank encoding template
    ///  Written by: Brad Barnhill
    /// </summary>
    public class Blank : BarcodeCommon, IBarcode
    {

        #region IBarcode Members

        public string Encoded_Value
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
