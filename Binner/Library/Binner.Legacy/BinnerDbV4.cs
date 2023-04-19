using Binner.Model.Common;

namespace Binner.Legacy
{
    public class BinnerDbV4 : IBinnerDb
    {
        public const byte VersionNumber = 4;
        public static DateTime VersionCreated = new DateTime(2023, 3, 13);

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
        /// Defined part types
        /// </summary>
        public ICollection<OAuthCredential> OAuthCredentials { get; set; } = new List<OAuthCredential>();

        /// <summary>
        /// Pcb - printed circuit boards (BOM)
        /// </summary>
        public ICollection<Pcb> Pcbs { get; set; } = new List<Pcb>();

        /// <summary>
        /// Pcb - printed circuit boards (BOM)
        /// </summary>
        public ICollection<PcbStoredFileAssignment> PcbStoredFileAssignments { get; set; } = new List<PcbStoredFileAssignment>();

        /// <summary>
        /// User defined Projects
        /// </summary>
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        /// <summary>
        /// Parts that are part of a project (BOM)
        /// </summary>
        public ICollection<ProjectPartAssignment> ProjectPartAssignments { get; set; } = new List<ProjectPartAssignment>();

        /// <summary>
        /// Pcb's that are part of a project (BOM)
        /// </summary>
        public ICollection<ProjectPcbAssignment> ProjectPcbAssignments { get; set; } = new List<ProjectPcbAssignment>();

        /// <summary>
        /// Defined part types
        /// </summary>
        public ICollection<PartType> PartTypes { get; set; } = new List<PartType>();

        /// <summary>
        /// Parts database
        /// </summary>
        public ICollection<Part> Parts { get; set; } = new List<Part>();

        /// <summary>
        /// Stored/uploaded files
        /// </summary>
        public ICollection<StoredFile> StoredFiles { get; set; } = new List<StoredFile>();

        /// <summary>
        /// OAuth requests
        /// </summary>
        public ICollection<OAuthRequest> OAuthRequests { get; set; } = new List<OAuthRequest>();

        /// <summary>
        /// A checksum for validating the database contents
        /// </summary>
        public string? Checksum { get; set; }

        public BinnerDbV4() { }

        /// <summary>
        /// Upgrade a database from a previous version
        /// </summary>
        /// <param name="previousDatabase"></param>
        /// <param name="buildChecksum"></param>
        public BinnerDbV4(BinnerDbV1 previousDatabase, Func<BinnerDbV4, string> buildChecksum)
        {
            Count = previousDatabase.Count;
            FirstPartId = previousDatabase.FirstPartId;
            LastPartId = previousDatabase.LastPartId;
            DateCreatedUtc = previousDatabase.DateCreatedUtc;
            DateModifiedUtc = previousDatabase.DateModifiedUtc;
            OAuthCredentials = previousDatabase.OAuthCredentials;
            Projects = previousDatabase.Projects;
            PartTypes = previousDatabase.PartTypes;
            Parts = previousDatabase.Parts;
            Checksum = buildChecksum(this);
        }

        /// <summary>
        /// Upgrade a database from a previous version
        /// </summary>
        /// <param name="previousDatabase"></param>
        /// <param name="buildChecksum"></param>
        public BinnerDbV4(BinnerDbV2 previousDatabase, Func<BinnerDbV4, string> buildChecksum)
        {
            Count = previousDatabase.Count;
            FirstPartId = previousDatabase.FirstPartId;
            LastPartId = previousDatabase.LastPartId;
            DateCreatedUtc= previousDatabase.DateCreatedUtc;
            DateModifiedUtc =  previousDatabase.DateModifiedUtc;
            OAuthCredentials = previousDatabase.OAuthCredentials;
            Projects = previousDatabase.Projects;
            PartTypes = previousDatabase.PartTypes;
            Parts = previousDatabase.Parts;
            StoredFiles = previousDatabase.StoredFiles;
            Checksum = buildChecksum(this);
        }

        /// <summary>
        /// Upgrade a database from a previous version
        /// </summary>
        /// <param name="previousDatabase"></param>
        /// <param name="buildChecksum"></param>
        public BinnerDbV4(BinnerDbV3 previousDatabase, Func<BinnerDbV4, string> buildChecksum)
        {
            Count = previousDatabase.Count;
            FirstPartId = previousDatabase.FirstPartId;
            LastPartId = previousDatabase.LastPartId;
            DateCreatedUtc= previousDatabase.DateCreatedUtc;
            DateModifiedUtc =  previousDatabase.DateModifiedUtc;
            OAuthCredentials = previousDatabase.OAuthCredentials;
            Projects = previousDatabase.Projects;
            PartTypes = previousDatabase.PartTypes;
            Parts = previousDatabase.Parts;
            StoredFiles = previousDatabase.StoredFiles;
            Checksum = buildChecksum(this);
        }
    }
}
