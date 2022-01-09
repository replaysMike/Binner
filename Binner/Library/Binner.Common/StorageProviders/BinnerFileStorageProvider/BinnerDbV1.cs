using Binner.Common.Models;
using System;
using System.Collections.Generic;

namespace Binner.Common.StorageProviders
{
    public class BinnerDbV1 : IBinnerDb
    {
        public const byte VersionNumber = 1;
        public static DateTime VersionCreated = new DateTime(2019, 10, 1);

        /// <summary>
        /// Number of parts in database
        /// </summary>
        public long Count { get; internal set; }

        /// <summary>
        /// The first part Id
        /// </summary>
        public long FirstPartId { get; internal set; }

        /// <summary>
        /// The last part Id
        /// </summary>
        public long LastPartId { get; internal set; }

        /// <summary>
        /// Date the database was created
        /// </summary>
        public DateTime DateCreatedUtc { get; internal set; }

        /// <summary>
        /// Date the database was last modified
        /// </summary>
        public DateTime DateModifiedUtc { get; internal set; }

        /// <summary>
        /// Defined part types
        /// </summary>
        public ICollection<OAuthCredential> OAuthCredentials { get; internal set; } = new List<OAuthCredential>();

        /// <summary>
        /// User defined Projects
        /// </summary>
        public ICollection<Project> Projects { get; internal set; } = new List<Project>();

        /// <summary>
        /// Defined part types
        /// </summary>
        public ICollection<PartType> PartTypes { get; internal set; } = new List<PartType>();

        /// <summary>
        /// Parts database
        /// </summary>
        public ICollection<Part> Parts { get; internal set; } = new List<Part>();

        /// <summary>
        /// A checksum for validating the database contents
        /// </summary>
        public string Checksum { get; internal set; }
    }
}
