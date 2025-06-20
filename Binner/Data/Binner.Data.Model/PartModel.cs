using Binner.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class PartModel : IEntity, IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PartModelId { get; set; }

        /// <summary>
        /// The associated part
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// Name of model
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Filename of model
        /// </summary>
        public string? Filename { get; set; } = string.Empty;

        /// <summary>
        /// Part model type
        /// </summary>
        public PartModelTypes ModelType { get; set; }

        /// <summary>
        /// The source where the model came from
        /// </summary>
        public PartModelSources Source { get; set; }

        /// <summary>
        /// Provided if the model has a url associated with it
        /// </summary>
        public string? Url { get; set; }

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

#if INITIALCREATE
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
#endif

        public Part? Part { get; set; }
    }
}
