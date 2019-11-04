using System.Collections.Generic;

namespace Binner.Common.Models
{
    public class UpdatePartRequest : PartBase
    {
        /// <summary>
        /// The part id
        /// </summary>
        public long PartId { get; set; }
    }
}
