using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.Models.Swarm.Responses
{
    public class SearchPartsResponse
    {
        /// <summary>
        /// The parts returned from the search
        /// </summary>
        public ICollection<PartNumber> Parts { get; set; } = new List<PartNumber>();
    }
}
