using System.Text.Json.Serialization;

namespace Binner.Model.KiCad
{
    /// <summary>
    /// KiCad part details
    /// </summary>
    /// <remarks>
    /// Ensure no properties are nullable, as they must return empty string or KiCad will ignore the data.
    /// </remarks>
    public class KiCadPartDetail : KiCadItem
    {
        [JsonPropertyName("symbolIdStr")]
        public string SymbolIdStr { get; set; } = string.Empty;

        [JsonPropertyName("exclude_from_bom")]
        public string ExcludeFromBom { get; set; } = "false";

        [JsonPropertyName("exclude_from_board")]
        public string ExcludeFromBoard { get; set; } = "false";

        [JsonPropertyName("exclude_from_sim")]
        public string ExcludeFromSim { get; set; } = "false";

        [JsonPropertyName("fields")]
        public IDictionary<string, KiCadValueVisibleItem> Fields { get; set; } = new Dictionary<string, KiCadValueVisibleItem>();
    }
}
