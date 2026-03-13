using System.Text.Json.Serialization;

namespace Binner.Model.KiCad
{
    /// <summary>
    /// KiCad value
    /// </summary>
    /// <remarks>
    /// Ensure no properties are nullable, as they must return empty string or KiCad will ignore the data.
    /// </remarks>
    public class KiCadValueVisibleItem
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
        
        [JsonPropertyName("visible")]
        public string Visible { get; set; } = "false"; // "true", "false" (why not bool KiCad lol?)

        public KiCadValueVisibleItem() { }
        public KiCadValueVisibleItem(string? value)
        {
            Value = value ?? string.Empty;
        }

        public KiCadValueVisibleItem(string? value, bool visible) : this(value)
        {
            Visible = visible ? "true" : "false";
        }
    }
}
