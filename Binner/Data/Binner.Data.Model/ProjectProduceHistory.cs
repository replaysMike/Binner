using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// Tracks the history when a project/BOM is produced
    /// </summary>
    public class ProjectProduceHistory : IEntity, IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProjectProduceHistoryId { get; set; }

        /// <summary>
        /// Project id being produced
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// The number of items produced
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// True to allow producing of unassociated parts
        /// </summary>
        public bool ProduceUnassociated { get; set; }

        /// <summary>
        /// Total parts consumed
        /// </summary>
        public int PartsConsumed { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        /// <summary>
        /// Associated user
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

        [ForeignKey(nameof(ProjectId))]
        public Project? Project { get; set; }

        public ICollection<ProjectPcbProduceHistory> ProjectPcbProduceHistory { get; set; } = null!;
    }
}
