using Binner.Global.Common;

namespace Binner.Model
{
    public class LegacyBinnerDb : IBinnerDb
    {
        public const byte VersionNumber = 7;
        public static DateTime VersionCreated = new DateTime(2023, 4, 7);

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
        /// Part suppliers
        /// </summary>
        public ICollection<PartSupplier> PartSuppliers { get; set; } = new List<PartSupplier>();

        /// <summary>
        /// A checksum for validating the database contents
        /// </summary>
        public string? Checksum { get; set; }

        /// <summary>
        /// 3D Models associated with a part
        /// </summary>
        public ICollection<PartModel> PartModels { get; set; } = new List<PartModel>();

        /// <summary>
        /// Parametrics associated with a part
        /// </summary>
        public ICollection<PartParametric> PartParametrics { get; set; } = new List<PartParametric>();

        /// <summary>
        /// Custom defined fields
        /// </summary>
        public ICollection<CustomField> CustomFields { get; set; } = new List<CustomField>();

        /// <summary>
        /// Custom defined field values
        /// </summary>
        public ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();

        public LegacyBinnerDb() { }
    }
}
