using System.Collections.Generic;

namespace Binner.Common.Models
{
    public class CreateBulkPartRequest
    {
        public ICollection<PartBase>? Parts { get; set; }
    }
}
