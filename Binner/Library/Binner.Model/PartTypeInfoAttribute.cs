namespace Binner.Model
{
    /// <summary>
    /// Indicates the parent of a part type
    /// </summary>
    public class PartTypeInfoAttribute : Attribute
    {
        /// <summary>
        /// Part type description
        /// </summary>
        public string? Description { get; }

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

        public PartTypeInfoAttribute(string? description, string? referenceDesignator, string? symbolId, string? keywords)
        {
            Description = description;
            ReferenceDesignator = referenceDesignator;
            SymbolId = symbolId;
            Keywords = keywords;
        }

        public PartTypeInfoAttribute(string? description)
        {
            Description = description;
        }

        public PartTypeInfoAttribute(string? description, string? referenceDesignator)
        {
            Description = description;
            ReferenceDesignator = referenceDesignator;
        }

        public PartTypeInfoAttribute(string? description, string? referenceDesignator, string? symbolId)
        {
            Description = description;
            ReferenceDesignator = referenceDesignator;
            SymbolId = symbolId;
        }
    }
}
