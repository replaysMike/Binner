using AnySerializer;
using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Common
{
    /// <summary>
    /// A pcb layout/design resource
    /// </summary>
    public class Pcb : IEntity, IEquatable<Pcb>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public long PcbId { get; set; }

        /// <summary>
        /// Pcb name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Pcb description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The serial format template. Example: SN000000000
        /// </summary>
        public string? SerialNumberFormat { get; set; }

        /// <summary>
        /// The last serial number used. Example: SN000000001
        /// </summary>
        public string? LastSerialNumber { get; set; }

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
        /// Quantity of PCBs that are produced on every production
        /// </summary>
        [PropertyVersion("BinnerDbV6")]
        public int Quantity { get; set; }

        /// <summary>
        /// Cost to produce a single PCB board
        /// </summary>
        [PropertyVersion("BinnerDbV6")]
        public double Cost { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Project project)
                return Equals(project);
            return false;
        }

        public bool Equals(Pcb? other)
        {
            return other != null && PcbId == other.PcbId && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return PcbId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(PcbId, UserId);
#endif

        }

        public override string ToString()
        {
            return $"{PcbId}: {Name}";
        }
    }
}
