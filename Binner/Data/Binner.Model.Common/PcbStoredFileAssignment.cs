using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Common
{
    /// <summary>
    /// A stored file associated with a pcb
    /// </summary>
    public class PcbStoredFileAssignment : IEntity, IEquatable<PcbStoredFileAssignment>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
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
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Modification date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is PcbStoredFileAssignment project)
                return Equals(project);
            return false;
        }

        public bool Equals(PcbStoredFileAssignment? other)
        {
            return other != null && PcbStoredFileAssignmentId == other.PcbStoredFileAssignmentId && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return PcbStoredFileAssignmentId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(PcbStoredFileAssignmentId, UserId);
#endif

        }
    }
}
