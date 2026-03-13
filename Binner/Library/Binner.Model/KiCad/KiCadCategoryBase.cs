using System.Text.Json.Serialization;

namespace Binner.Model.KiCad
{
    /// <summary>
    /// KiCad Category
    /// </summary>
    /// <remarks>
    /// Ensure no properties are nullable, as they must return empty string or KiCad will ignore the data.
    /// </remarks>
    public class KiCadCategoryBase : KiCadItem
    {
        /// <summary>
        /// Description of item
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
