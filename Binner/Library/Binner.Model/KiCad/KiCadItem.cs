using System.Text.Json.Serialization;

namespace Binner.Model.KiCad
{
    /// <summary>
    /// KiCad item
    /// </summary>
    /// <remarks>
    /// Ensure no properties are nullable, as they must return empty string or KiCad will ignore the data.
    /// </remarks>
    public class KiCadItem
    {
        /// <summary>
        /// Record Id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of item
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
