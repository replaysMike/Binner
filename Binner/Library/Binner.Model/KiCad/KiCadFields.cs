using System.Text.Json.Serialization;

namespace Binner.Model.KiCad
{
    public class KiCadFields
    {
        [JsonPropertyName("footprint")]
        public KiCadValueVisibleItem Footprint { get; set; } = new();

        [JsonPropertyName("datasheet")]
        public KiCadValueVisibleItem Datasheet { get; set; } = new();

        [JsonPropertyName("value")]
        public KiCadValueItem Value { get; set; } = new();

        [JsonPropertyName("reference")]
        public KiCadValueItem Reference { get; set; } = new();

        [JsonPropertyName("description")]
        public KiCadValueVisibleItem Description { get; set; } = new();

        [JsonPropertyName("keywords")]
        public KiCadValueVisibleItem Keywords { get; set; } = new();

        [JsonPropertyName("custom1")]
        public KiCadValueVisibleItem? Custom1 { get; set; }

        [JsonPropertyName("custom2")]
        public KiCadValueVisibleItem? Custom2 { get; set; }

        [JsonPropertyName("custom3")]
        public KiCadValueVisibleItem? Custom3 { get; set; }

        [JsonPropertyName("digiKey")]
        public KiCadValueVisibleItem DigiKey { get; set; } = new();

        [JsonPropertyName("mouser")]
        public KiCadValueVisibleItem Mouser { get; set; } = new();

        [JsonPropertyName("arrow")]
        public KiCadValueVisibleItem Arrow { get; set; } = new();

        [JsonPropertyName("tme")]
        public KiCadValueVisibleItem Tme { get; set; } = new();

        [JsonPropertyName("element14")]
        public KiCadValueVisibleItem Element14 { get; set; } = new();

        [JsonPropertyName("extensionValue1")]
        public KiCadValueVisibleItem ExtensionValue1 { get; set; } = new();

        [JsonPropertyName("extensionValue2")]
        public KiCadValueVisibleItem ExtensionValue2 { get; set; } = new();
    }
}
