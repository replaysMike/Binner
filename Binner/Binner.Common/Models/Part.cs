using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        [Key]
        public long PartId { get; set; }

        /// <summary>
        /// The number of items in stock
        /// </summary>
        public long Quantity { get; set; }

        /// <summary>
        /// The part should be reordered when it gets below this value
        /// </summary>
        public int LowStockThreshold { get; set; } = SystemDefaults.LowStockThreshold;

        /// <summary>
        /// The main part number
        /// </summary>
        [MaxLength(64)]
        public string PartNumber { get; set; }

        /// <summary>
        /// The Digikey part number
        /// </summary>
        public string DigiKeyPartNumber { get; set; }

        /// <summary>
        /// The Mouser part number
        /// </summary>
        public string MouserPartNumber { get; set; }

        /// <summary>
        /// Description of part
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of part
        /// </summary>
        public long PartTypeId { get; set; }

        /// <summary>
        /// Project associated with the part
        /// </summary>
        public long? ProjectId { get; set; }

        /// <summary>
        /// Additional keywords
        /// </summary>
        public ICollection<string> Keywords { get; set; }

        /// <summary>
        /// Datasheet URL
        /// </summary>
        public string DatasheetUrl { get; set; }

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

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Part);
        }

        public bool Equals(Part other)
        {
            return other != null && PartId == other.PartId && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return PartId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(PartId, UserId);
#endif
        }

        public override string ToString()
        {
            return $"{PartId}: {PartNumber} - {Description}";
        }
    }
}
