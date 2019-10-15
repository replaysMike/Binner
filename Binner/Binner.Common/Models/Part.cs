using System;
using System.Collections.Generic;

namespace Binner.Common.Models
{
    /// <summary>
    /// A Part
    /// </summary>
    public class Part
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// The number of items in stock
        /// </summary>
        public long Quantity { get; set; }

        /// <summary>
        /// The part should be reordered when it gets below this value|
        /// </summary>
        public int LowStockThreshold { get; set; } = SystemDefaults.LowStockThreshold;

        /// <summary>
        /// The main part number
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// The Digikey part number
        /// </summary>
        public string DigiKeyPartNumber { get; set; }

        /// <summary>
        /// Description of part
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of part
        /// </summary>
        public long PartTypeId { get; set; }

        /// <summary>
        /// Additional keywords
        /// </summary>
        public ICollection<string> Keywords { get; set; }

        /// <summary>
        /// Datasheet URL
        /// </summary>
        public string DatasheetUrl { get; set; }

        /// <summary>
        /// Project associated with the part
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Location of part (i.e. warehouse, room)
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Bin number (i.e. Shelf)
        /// </summary>
        public string BinNumber { get; set; }

        /// <summary>
        /// Secondary Bin number (i.e. Bin)
        /// </summary>
        public string BinNumber2 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Part);
        }

        public bool Equals(Part other)
        {
            return other != null && PartId == other.PartId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PartId);
        }

        public override string ToString()
        {
            return $"{PartId}: {PartNumber} - {Description}";
        }
    }
}
