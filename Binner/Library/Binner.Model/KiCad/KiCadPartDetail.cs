using System.Text.Json.Serialization;

namespace Binner.Model.KiCad
{
    public class KiCadPartDetail : KiCadItem
    {
        public string SymbolIdStr { get; set; } = string.Empty;

        [JsonPropertyName("exclude_from_bom")]
        public string ExcludeFromBom { get; set; } = "false";

        [JsonPropertyName("exclude_from_board")]
        public string ExcludeFromBoard { get; set; } = "false";

        [JsonPropertyName("exclude_from_sim")]
        public string ExcludeFromSim { get; set; } = "false";

        public IDictionary<string, KiCadValueVisibleItem> Fields { get; set; } = new Dictionary<string, KiCadValueVisibleItem>();
    }
}
