using System;
using System.ComponentModel.DataAnnotations;

namespace Binner.Common.Models
{
    public class ProjectPart
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
        /// The quantity of parts available, used when a part is not associated with this entry (BOM).
        /// Otherwise, the part's quantity (in stock) should be used.
        /// </summary>
        public int QuantityAvailable { get; set; }

        /// <summary>
        /// Notes about this part
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Custom reference Id that can be used as a custom designator for the part
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Custom reference Id that can be used as a custom designator for the part on the PCB (silkscreen values)
        /// </summary>
        public string? SchematicReferenceId { get; set; }

        public string? CustomDescription { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Modification date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; } = DateTime.UtcNow;

        public PartResponse? Part { get; set; }
    }
}
