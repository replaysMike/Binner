using System;
using System.Collections.Generic;
using System.Text;

namespace Binner.Model.Requests
{
    public class PrintBrotherImageLabelRequest
    {
        public string ImageBase64 { get; set; }
        public string ImageContentType { get; set; }
        public double LabelLengthMm { get; set; }
        public int Quantity { get; set; }
        public double TapeWidthMm { get; set; }
        public string Token { get; set; }
    }
}
