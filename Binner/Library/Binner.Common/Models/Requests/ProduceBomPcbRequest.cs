using System.Collections.Generic;

namespace Binner.Common.Models.Requests
{
    public class ProduceBomPcbRequest
    {
        public long ProjectId { get; set; }
        public int Quantity { get; set; }

        /// <summary>
        /// Process unassociated parts
        /// </summary>
        public bool Unassociated { get; set; }

        /// <summary>
        /// List of PCBs to process
        /// </summary>
        public ICollection<ProducePcb> Pcbs { get; set; } = new List<ProducePcb>();
    }

    public class ProducePcb
    {
        public long PcbId { get; set; }
        public string? SerialNumber { get; set; }
    }
}
