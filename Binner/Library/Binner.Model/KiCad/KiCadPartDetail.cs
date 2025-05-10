using System.Text.Json.Serialization;

namespace Binner.Model.KiCad
{
    public class KiCadPartDetail : KiCadItem
    {
        public string SymbolIdStr { get; set; } = string.Empty;

        [JsonPropertyName("exclude_from_bom")]
        public string ExcludeFromBom { get; set; } = string.Empty;

        [JsonPropertyName("exclude_from_board")]
        public string ExcludeFromBoard { get; set; } = string.Empty;

        [JsonPropertyName("exclude_from_sim")]
        public string ExcludeFromSim { get; set; } = string.Empty;

        public KiCadFields Fields { get; set; } = new();
    }
}
