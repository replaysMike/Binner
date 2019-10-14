using System;
using System.Collections.Generic;
using System.Text;

namespace Binner.Common.Models
{
    /// <summary>
    /// Metadata about a part number
    /// </summary>
    public class PartMetadata
    {
        /// <summary>
        /// Part number
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Datasheet Url
        /// </summary>
        public string DatasheetUrl { get; set; }

        /// <summary>
        /// The part description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The part's cost
        /// </summary>
        public decimal Cost { get; set; }
    }
}
