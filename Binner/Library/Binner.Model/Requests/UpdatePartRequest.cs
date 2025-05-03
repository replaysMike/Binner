﻿using Binner.Model.Barcode;

namespace Binner.Model.Requests
{
    public class UpdatePartRequest : PartBase
    {
        /// <summary>
        /// The part id
        /// </summary>
        public long PartId { get; set; }

        public BarcodeScan? BarcodeObject { get; set; }
    }
}
