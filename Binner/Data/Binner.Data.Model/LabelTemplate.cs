using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class LabelTemplate : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LabelTemplateId { get; set; }

        /// <summary>
        /// Name of label template
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Width of label in inches
        /// </summary>
        public string Width { get; set; } = null!;

        /// <summary>
        /// Height of label in inches
        /// </summary>
        public string Height { get; set; } = null!;

        /// <summary>
        /// Label margins in the format of "0 0 0 0" or "0" or "0 0"
        /// </summary>
        public string Margin { get; set; } = null!;

        /// <summary>
        /// Dpi to use for template
        /// </summary>
        public int Dpi { get; set; }

        /// <summary>
        /// The paper source (left or right, if applicable)
        /// </summary>
        public int LabelPaperSource { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        public ICollection<Label>? Labels { get; set; }
    }
}
