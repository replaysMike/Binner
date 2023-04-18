using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// Tracks the history when a project/BOM is produced
    /// </summary>
    public class ProjectPcbProduceHistory : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProjectPcbProduceHistoryId { get; set; }

        /// <summary>
        /// The parent production record
        /// </summary>
        public long ProjectProduceHistoryId { get; set; }

        /// <summary>
        /// Pcb that was produced
        /// </summary>
        public long? PcbId { get; set; }

        /// <summary>
        /// Quantity of PCBs that are produced on every production (snapshot of PCB value)
        /// </summary>
        public int PcbQuantity { get; set; }

        /// <summary>
        /// Cost to produce a single PCB board (snapshot of PCB value)
        /// </summary>
        public double PcbCost { get; set; }

        /// <summary>
        /// The serial number of the produced pcb (or null for unassociated productions)
        /// </summary>
        public string? SerialNumber { get; set; }

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

        [ForeignKey(nameof(PcbId))]
        public Pcb? Pcb { get; set; }

        [ForeignKey(nameof(ProjectProduceHistoryId))]
        public ProjectProduceHistory? ProjectProduceHistory { get; set; }
    }
}
