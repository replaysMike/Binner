using System;
using System.Collections.Generic;
using System.Text;

namespace Binner.Common.Models
{
    public class PartResults
    {
        public ICollection<CommonPart> Parts { get; set; } = new List<CommonPart>();
    }

    public class CommonPart
    {
        public string Supplier { get; set; }
        public string SupplierPartNumber { get; set; }
        public string BasePartNumber { get; set; }
        public ICollection<string> AdditionalPartNumbers { get; set; } = new List<string>();
        public string Manufacturer { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public decimal Cost { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public ICollection<string> DataSheetUrls { get; set; } = new List<string>();
        public string PartType { get; set; }
        public string MountingType { get; set; }
        public ICollection<string> Keywords { get; set; } = new List<string>();
        public string ImageUrl { get; set; }
        public string ProductUrl { get; set; }
        public string Package { get; set; }
        public string Status { get; set; }
    }
}
