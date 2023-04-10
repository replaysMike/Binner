using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// A pcb layout/design resource
    /// </summary>
    public class Pcb : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        /// Quantity of PCBs that are produced on every production
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Cost to produce a single PCB board
        /// </summary>
        public double Cost { get; set; }

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
        public int UserId { get; set; }

        public ICollection<PcbStoredFileAssignment>? PcbStoredFileAssignments { get; set; }
        public ICollection<ProjectPcbAssignment>? ProjectPcbAssignments { get; set; }
    }
}
