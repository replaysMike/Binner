using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// Defines a type of part or category/sub-category
    /// </summary>
    public class PartType
#if INITIALCREATE
        : IEntity,
#else
        : IPartialEntity,
#endif
        IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PartTypeId { get; set; }

        /// <summary>
        /// Associated user for user defined types, null for system defined types
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// If this is a child type, indicates the parent type
        /// </summary>
        public long? ParentPartTypeId { get; set; }

        /// <summary>
        /// The name of the part type
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
        /// Name or SVG content of icon.
        /// If left empty, default icon choices will be applied.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

#if INITIALCREATE
        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
#endif

        [ForeignKey(nameof(ParentPartTypeId))]
        public PartType? ParentPartType { get; set; }

        public ICollection<Part> Parts { get; set; } = null!;
    }
}
