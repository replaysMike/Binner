using System.Collections.Generic;

namespace Binner.Common.Integrations.Models.Mouser
{
    public class SearchResults
    {
        public int NumberOfResult { get; set; }
        public ICollection<MouserPart>? Parts { get; set; } = new List<MouserPart>();
    }
}
