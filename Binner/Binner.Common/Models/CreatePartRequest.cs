using System.Collections.Generic;

namespace Binner.Common.Models
{
    public class CreatePartRequest
    {
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
        public PartType PartType { get; set; }

        /// <summary>
        /// Additional keywords
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// Datasheet URL
        /// </summary>
        public string DatasheetUrl { get; set; }

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
