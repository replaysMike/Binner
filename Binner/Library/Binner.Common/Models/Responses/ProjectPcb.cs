using System;

namespace Binner.Common.Models
{
    public class ProjectPcb
    {
        /// <summary>
        /// Primary key
        /// </summary>
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
        /// Number of parts assigned to pcb
        /// </summary>
        public int PartsCount { get; set; }
    }
}
