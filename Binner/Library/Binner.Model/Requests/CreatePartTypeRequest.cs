namespace Binner.Model.Requests
{
    public class CreatePartTypeRequest
    {
        /// <summary>
        /// Name of Part Type
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Name or SVG content of icon
        /// If left empty, default icon choices will be applied.
        /// </summary>
        public string? Icon { get;set; }

        /// <summary>
        /// Description of project
        /// </summary>
        public long? ParentPartTypeId { get; set; }

        /// <summary>
        /// Part type description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Reference designator
        /// </summary>
        public string? ReferenceDesignator { get; set; }

        /// <summary>
        /// The symbol id of the part type (KiCad)
        /// </summary>
        public string? SymbolId { get; set; }
    }
}
