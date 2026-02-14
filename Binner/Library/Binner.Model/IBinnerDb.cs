using Binner.Global.Common;

namespace Binner.Model
{
    public interface IBinnerDb
    {
        /// <summary>
        /// Number of parts in database
        /// </summary>
        long Count { get; }

        /// <summary>
        /// The first part Id
        /// </summary>
        long FirstPartId { get; }

        /// <summary>
        /// The last part Id
        /// </summary>
        long LastPartId { get; }

        /// <summary>
        /// Date the database was created
        /// </summary>
        DateTime DateCreatedUtc { get; }

        /// <summary>
        /// Date the database was last modified
        /// </summary>
        DateTime DateModifiedUtc { get; }

        /// <summary>
        /// OAuth stored credentials
        /// </summary>
        ICollection<OAuthCredential> OAuthCredentials { get; }

        /// <summary>
        /// User defined Projects
        /// </summary>
        ICollection<Project> Projects { get; }

        /// <summary>
        /// Defined part types
        /// </summary>
        ICollection<PartType> PartTypes { get; }

        /// <summary>
        /// Parts database
        /// </summary>
        ICollection<Part> Parts { get; }

        /// <summary>
        /// 3D Models associated with a part
        /// </summary>
        ICollection<PartModel> PartModels { get; }

        /// <summary>
        /// Parametrics associated with a part
        /// </summary>
        ICollection<PartParametric> PartParametrics { get; }

        /// <summary>
        /// Custom defined fields
        /// </summary>
        ICollection<CustomField> CustomFields { get; }

        /// <summary>
        /// Custom defined field values
        /// </summary>
        ICollection<CustomFieldValue> CustomFieldValues { get; }

        /// <summary>
        /// Pcb (BOM)
        /// </summary>
        ICollection<Pcb> Pcbs { get; }

        /// <summary>
        /// Project pcb assignments (BOM)
        /// </summary>
        ICollection<ProjectPcbAssignment> ProjectPcbAssignments { get; }
        
        /// <summary>
        /// Project part assignments (BOM)
        /// </summary>
        ICollection<ProjectPartAssignment> ProjectPartAssignments { get; }

        /// <summary>
        /// A checksum for validating the database contents
        /// </summary>
        string? Checksum { get; }

    }
}
