using Binner.Global.Common;

namespace Binner.Model.Responses
{
    /// <summary>
    /// A user defined project
    /// </summary>
    public class PartTypeResponse
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long PartTypeId { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string? Name { get; set; }

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

        /// <summary>
        /// Optional keywords to help with search
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Name or SVG content of icon
        /// If left empty, default icon choices will be applied.
        /// </summary>
        public string? Icon { get;set; }

        /// <summary>
        /// The parent part type
        /// </summary>
        public long? ParentPartTypeId { get; set; }

        public string? ParentPartType { get; set; }

        /// <summary>
        /// The number of parts assigned to the project
        /// </summary>
        public long Parts { get; set; }

        public ICollection<CustomValue> CustomFields { get; set; } = new List<CustomValue>();

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
