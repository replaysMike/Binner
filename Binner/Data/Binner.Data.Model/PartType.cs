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
        IUserData
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
