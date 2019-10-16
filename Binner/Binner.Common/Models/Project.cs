using System;

namespace Binner.Common.Models
{
    /// <summary>
    /// A user defined project
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Project description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Project creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Project);
        }

        public bool Equals(Project other)
        {
            return other != null && ProjectId == other.ProjectId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return ProjectId.GetHashCode();
#else
            return HashCode.Combine(ProjectId);
#endif

        }

        public override string ToString()
        {
            return $"{ProjectId}: {Name}";
        }
    }
}
