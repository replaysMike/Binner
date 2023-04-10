using AnySerializer;
using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Common
{
    public class ProjectPartAssignment : IEntity, IEquatable<ProjectPartAssignment>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public long ProjectPartAssignmentId { get; set; }

        /// <summary>
        /// The associated project id
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// The associated part id (optional, if using PartName)
        /// </summary>
        public long? PartId { get; set; }

        /// <summary>
        /// The associated pcb id (optional)
        /// </summary>
        public long? PcbId { get; set; }

        /// <summary>
        /// If a part number isn't matched, a custom part name can be added to identify the part by name only
        /// </summary>
        public string? PartName { get; set; }

        /// <summary>
        /// The quantity of parts needed (BOM)
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Notes about this part
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Custom reference Id that can be used as a custom designator for the part
        /// </summary>
        public string? ReferenceId { get; set; }

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

        /// <summary>
        /// The quantity of parts available, used when a part is not associated with this entry (BOM).
        /// Otherwise, the part's quantity (in stock) should be used.
        /// </summary>
        [PropertyVersion("BinnerDbV5")]
        public int QuantityAvailable { get; set; }

        /// <summary>
        /// Custom reference Id that can be used as a custom designator for the part on the PCB (silkscreen values)
        /// </summary>
        [PropertyVersion("BinnerDbV6")]
        public string? SchematicReferenceId { get; set; }

        [PropertyVersion("BinnerDbV6")]
        public string? CustomDescription { get; set; }

        /// <summary>
        /// Cost of part
        /// </summary>
        [PropertyVersion("BinnerDbV7")]
        public double Cost { get; set; }

        /// <summary>
        /// Currency of part
        /// </summary>
        [PropertyVersion("BinnerDbV7")]
        public string? Currency { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Project project)
                return Equals(project);
            return false;
        }

        public bool Equals(ProjectPartAssignment? other)
        {
            return other != null && ProjectPartAssignmentId == other.ProjectPartAssignmentId && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return ProjectPartAssignmentId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(ProjectPartAssignmentId, UserId);
#endif

        }
    }
}
