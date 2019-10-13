using Binner.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Binner.Common.StorageProviders
{
    public class BinnerDb
    {
        /// <summary>
        /// Database version
        /// </summary>
        public byte Version { get; internal set; }

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
        /// Parts database
        /// </summary>
        public ICollection<Part> Parts { get; internal set; } = new List<Part>();

        /// <summary>
        /// A checksum for validating the database contents
        /// </summary>
        public string Checksum { get; internal set; }
    }
}
