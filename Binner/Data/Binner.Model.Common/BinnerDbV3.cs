namespace Binner.Model.Common
{
    public class BinnerDbV3 : IBinnerDb
    {
        public const byte VersionNumber = 3;
        public static DateTime VersionCreated = new DateTime(2023, 2, 10);

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

        public BinnerDbV3() { }

        /// <summary>
        /// Upgrade a database from a previous version
        /// </summary>
        /// <param name="previousDatabase"></param>
        /// <param name="buildChecksum"></param>
        public BinnerDbV3(BinnerDbV1 previousDatabase, Func<BinnerDbV3, string> buildChecksum)
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
        public BinnerDbV3(BinnerDbV2 previousDatabase, Func<BinnerDbV3, string> buildChecksum)
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
