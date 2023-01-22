using System;
using System.Collections.Generic;

namespace Binner.Common.Models.Responses
{
    public class ExternalOrderResponse
    {
        public string OrderId { get; set; }
        public string Supplier { get; set; }
        public DateTime OrderDate { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string CustomerId { get; set; }
        public string TrackingNumber { get; set; }
        public ICollection<CommonPart> Parts { get; set; } = new List<CommonPart>();
    }
}
