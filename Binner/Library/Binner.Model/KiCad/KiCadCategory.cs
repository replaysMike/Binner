namespace Binner.Model.KiCad
{
    public class KiCadCategory : KiCadItem
    {
        /// <summary>
        /// Description of item
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Reference Designator. 'R' for resistors
        /// </summary>
        public string? ReferenceDesignator { get; set; }

        /// <summary>
        /// Symbol Id. 'Device:R' for resistors
        /// </summary>
        public string? SymbolId { get; set; }
    }
}
