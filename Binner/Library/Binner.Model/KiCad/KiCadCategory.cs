using System.Text.Json.Serialization;

namespace Binner.Model.KiCad
{
    /// <summary>
    /// KiCad Category
    /// </summary>
    /// <remarks>
    /// Ensure no properties are nullable, as they must return empty string or KiCad will ignore the data.
    /// </remarks>
    public class KiCadCategory : KiCadCategoryBase
    {
        /// <summary>
        /// Reference Designator. 'R' for resistors
        /// </summary>
        [JsonPropertyName("referenceDesignator")]
        public string ReferenceDesignator { get; set; } = string.Empty;

        /// <summary>
        /// Symbol Id. 'Device:R' for resistors
        /// </summary>
        [JsonPropertyName("symbolId")]
        public string SymbolId { get; set; } = string.Empty;
    }
}
