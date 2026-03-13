using System.Text.Json.Serialization;

namespace Binner.Model.KiCad
{
    /// <summary>
    /// KiCad value
    /// </summary>
    /// <remarks>
    /// Ensure no properties are nullable, as they must return empty string or KiCad will ignore the data.
    /// </remarks>
    public class KiCadValueItem
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        public KiCadValueItem() { }
        public KiCadValueItem(string? value)
        {
            Value = value ?? string.Empty;
        }
    }
}
