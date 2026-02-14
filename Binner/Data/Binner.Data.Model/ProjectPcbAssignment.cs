using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class ProjectPcbAssignment 
#if INITIALCREATE
        : IEntity,
#else
        : IPartialEntity, 
#endif
        IOptionalUserData, IGlobalData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProjectPcbAssignmentId { get; set; }

        public Guid GlobalId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The associated project id
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// The associated pcb id
        /// </summary>
        public long PcbId { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

#if INITIALCREATE
        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
#endif

        public Pcb? Pcb { get; set; }

        public Project? Project { get; set; }
    }
}
