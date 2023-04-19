using System;

namespace Binner.Legacy.StorageProviders
{
    public class BinnerDbVersion
    {
        /// <summary>
        /// The BinnerDb Version
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// The creation date of this version
        /// </summary>
        public DateTime Created { get; set; }

        public BinnerDbVersion() { }

        public BinnerDbVersion(byte version, DateTime created)
        {
            Version = version;
            Created = created;
        }
    }
}
