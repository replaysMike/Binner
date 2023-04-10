using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Common
{
    public class ProjectPcbAssignment : IEntity, IEquatable<ProjectPcbAssignment>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public long ProjectPcbAssignmentId { get; set; }

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
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Project project)
                return Equals(project);
            return false;
        }

        public bool Equals(ProjectPcbAssignment? other)
        {
            return other != null && ProjectPcbAssignmentId == other.ProjectPcbAssignmentId && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return ProjectPcbAssignmentId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(ProjectPcbAssignmentId, UserId);
#endif

        }
    }
}
