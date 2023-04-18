using System;

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
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }
    }
}
