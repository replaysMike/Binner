using System.Collections.Generic;

namespace Binner.Common.Models
{
    public class PartResults
    {
        /// <summary>
        /// List of matching parts
        /// </summary>
        public ICollection<CommonPart> Parts { get; set; } = new List<CommonPart>();
    }
}
