using Binner.Model.Common;

namespace Binner.Legacy
{
    public class BinnerDbV1 : IBinnerDb
    {
        public const byte VersionNumber = 1;
        public static DateTime VersionCreated = new DateTime(2022, 1, 1);

        /// <summary>
        /// Number of parts in database
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// The first part Id
        /// </summary>
        public long FirstPartId { get; set; }

        /// <summary>
        /// The last part Id
        /// </summary>
        public long LastPartId { get; set; }

        /// <summary>
        /// Date the database was created
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Date the database was last modified
        /// </summary>
        public DateTime DateModifiedUtc { get; set; }

        /// <summary>
        /// OAuth credentials
        /// </summary>
        public ICollection<OAuthCredential> OAuthCredentials { get; set; } = new List<OAuthCredential>();

        /// <summary>
        /// User defined Projects
        /// </summary>
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        /// <summary>
        /// Defined part types
        /// </summary>
        public ICollection<PartType> PartTypes { get; set; } = new List<PartType>();

        /// <summary>
        /// Parts database
        /// </summary>
        public ICollection<Part> Parts { get; set; } = new List<Part>();

        /// <summary>
        /// A checksum for validating the database contents
        /// </summary>
        public string? Checksum { get; set; }
    }
}
