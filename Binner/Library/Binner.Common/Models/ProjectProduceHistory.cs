using System;
using System.Collections;
using System.Collections.Generic;

namespace Binner.Common.Models
{
    public class ProjectProduceHistory
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long ProjectProduceHistoryId { get; set; }

        /// <summary>
        /// Project id being produced
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// The number of items produced
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// True to allow producing of unassociated parts
        /// </summary>
        public bool ProduceUnassociated { get; set; }

        /// <summary>
        /// Total parts consumed
        /// </summary>
        public int PartsConsumed { get; set; }

        /// <summary>
        /// List of PCBs that were produced
        /// </summary>
        public ICollection<ProjectPcbProduceHistory> Pcbs { get; set; } = new List<ProjectPcbProduceHistory>();

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }
    }
}
