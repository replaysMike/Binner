using System.Collections;
using System.Collections.Generic;

namespace Binner.Common.Models.Requests
{
    public class RemoveBomPartRequest
    {
        public string? Project { get; set; }
        public int? ProjectId { get; set; }

        public ICollection<long> Ids { get; set; } = new List<long>();
    }
}
