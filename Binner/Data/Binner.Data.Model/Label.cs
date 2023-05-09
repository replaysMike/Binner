using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class Label : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LabelId { get; set; }

        /// <summary>
        /// The label template to use (defines the label size)
        /// </summary>
        public int LabelTemplateId { get; set; }

        /// <summary>
        /// Template stored as JSON
        /// </summary>
        public string Template { get; set; } = null!;

        /// <summary>
        /// If set to true, this record is used for the part label template
        /// </summary>
        public bool IsPartLabelTemplate { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey(nameof(LabelTemplateId))]
        public LabelTemplate? LabelTemplate { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }
    }
}
