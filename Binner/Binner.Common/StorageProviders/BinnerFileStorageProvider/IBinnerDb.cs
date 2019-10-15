using Binner.Common.Models;
using System;
using System.Collections.Generic;

namespace Binner.Common.StorageProviders
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
        /// Defined part types
        /// </summary>
        ICollection<PartType> PartTypes { get; }

        /// <summary>
        /// Parts database
        /// </summary>
        ICollection<Part> Parts { get; }

        /// <summary>
        /// A checksum for validating the database contents
        /// </summary>
        string Checksum { get; }

    }
}
