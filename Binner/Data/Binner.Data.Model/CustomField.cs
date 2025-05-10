using Binner.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// A user-defined custom field that can be associated with a record held in CustomFieldValues
    /// </summary>
    public class CustomField : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CustomFieldId { get; set; }

        /// <summary>
        /// The type of data this custom field holds
        /// </summary>
        public CustomFieldTypes CustomFieldTypeId { get; set; }

        /// <summary>
        /// Name of the custom field
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Description of the custom field
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Modification date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; }

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        public ICollection<CustomFieldValue>? CustomFieldValues { get; set; }
    }
}
