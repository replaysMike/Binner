using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// A stored file associated with a pcb
    /// </summary>
    public class PcbStoredFileAssignment : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PcbStoredFileAssignmentId { get; set; }

        public long PcbId { get; set; }

        public long StoredFileId { get; set; }

        /// <summary>
        /// A name for the stored file
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Notes for the stored file
        /// </summary>
        public string? Notes { get; set; }

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

#if INITIALCREATE
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
#endif

        public Pcb? Pcb { get; set; }

        public StoredFile? StoredFile { get; set; }
    }
}
