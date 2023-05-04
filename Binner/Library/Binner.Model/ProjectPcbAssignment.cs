namespace Binner.Model
{
    public class ProjectPcbAssignment : IEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
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
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int UserId { get; set; }

        public Pcb? Pcb { get; set; }

        public Project? Project { get; set; }
    }
}
