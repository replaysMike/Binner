using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// Defines a type of part or category/sub-category
    /// </summary>
    public class PartType : IEntity
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
        /// If this is a child type, indicates the parent type
        /// </summary>
        public long? ParentPartTypeId { get; set; }

        /// <summary>
        /// The name of the part type
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(ParentPartTypeId))]
        public PartType? ParentPartType { get; set; }

        public ICollection<Part> Parts { get; set; } = null!;
    }
}
