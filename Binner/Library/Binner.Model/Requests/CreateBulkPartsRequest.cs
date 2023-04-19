using System.Collections.Generic;

namespace Binner.Model.Requests
{
    public class CreateBulkPartRequest
    {
        public ICollection<PartBase>? Parts { get; set; }
    }
}
