using System.Collections.Generic;

namespace Binner.Common.Models
{
    public class UpdatePartRequest
    {
        /// <summary>
        /// The part id
        /// </summary>
        public int PartId { get; set; }

        /// <summary>
        /// The main part number
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Quantity on hand
        /// </summary>
        public long Quantity { get; set; }

        /// <summary>
        /// The part should be reordered when it gets below this value
        /// </summary>
        public int LowStockThreshold { get; set; }

        /// <summary>
        /// Project Id
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        /// The optional Digikey part number
        /// </summary>
        public string DigiKeyPartNumber { get; set; }

        /// <summary>
        /// The optional Mouser part number
        /// </summary>
        public string MouserPartNumber { get; set; }

        /// <summary>
        /// Description of part
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of part
        /// </summary>
        public string PartType { get; set; }

        /// <summary>
        /// Additional keywords
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// Datasheet URL
        /// </summary>
        public string DatasheetUrl { get; set; }

        /// <summary>
        /// Location of part (i.e. warehouse, room)
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Bin number
        /// </summary>
        public string BinNumber { get; set; }

        /// <summary>
        /// Secondary Bin number
        /// </summary>
        public string BinNumber2 { get; set; }
    }
}
