using Newtonsoft.Json;

namespace Binner.Model.BinnerBin
{
    public class BinnerBinConfig
    {
        [JsonProperty("location")]
        public string Location { get; set; } = string.Empty;

        [JsonProperty("binNumber")]
        public string BinNumber { get; set; } = string.Empty;

        [JsonProperty("binNumber2")]
        public string? BinNumber2 { get; set; } = string.Empty;

        public BinnerBinConfig() { }

        public BinnerBinConfig(string location, string binNumber, string binNumber2)
        {
            Location = location;
            BinNumber = binNumber;
            BinNumber2 = binNumber2;
        }

        public override int GetHashCode()             
        {  
            if (BinNumber2 != null) {
                return HashCode.Combine(Location.GetHashCode(), BinNumber.GetHashCode(), BinNumber2.GetHashCode()); 
            }

            return HashCode.Combine(Location.GetHashCode(), BinNumber.GetHashCode()); 
        }

        public override bool Equals(object obj) 
        { 
            return Equals(obj as BinnerBinConfig); 
        }

        public bool Equals(BinnerBinConfig obj)
        { 
            return obj != null && obj.Location == this.Location && obj.BinNumber == this.BinNumber && obj.BinNumber2 == this.BinNumber2; 
        }
    }
}
