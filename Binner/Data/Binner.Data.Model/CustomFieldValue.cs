using Binner.Global.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class CustomFieldValue : IEntity, IOptionalUserData, IGlobalData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CustomFieldValueId { get; set; }

        public Guid GlobalId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The field that this value is associated with
        /// </summary>
        public long CustomFieldId { get; set; }

        /// <summary>
        /// The type of data this custom field holds.
        /// Duplicated to make it easier to query
        /// </summary>
        public CustomFieldTypes CustomFieldTypeId { get; set; }

        /// <summary>
        /// The primary key record that this value is associated with
        /// </summary>
        public long RecordId { get; set; }

        /// <summary>
        /// The value of the custom field
        /// </summary>
        public string? Value { get; set; }

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

        [ForeignKey(nameof(CustomFieldId))]
        public CustomField? CustomField { get; set; }
    }
}
