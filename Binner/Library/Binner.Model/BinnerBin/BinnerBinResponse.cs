using Newtonsoft.Json;

namespace Binner.Model.BinnerBin
{
    public class BinnerBinResponse
    {
        [JsonProperty("color")]
        public string Color { get; set; } = string.Empty;

        [JsonProperty("binNumber")]
        public string BinNumber { get; set; } = string.Empty;

        public BinnerBinResponse() { }
        public BinnerBinResponse(string binNumber, string color) 
        { 
            Color = color;
            BinNumber = binNumber;
        }
    }
}
