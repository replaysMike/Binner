using System.Collections.Generic;

namespace Binner.Common.IO
{
    public class ImportResult
    {
        /// <summary>
        /// True if operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Number of rows imported
        /// </summary>
        public int TotalRowsImported { get; set; }

        /// <summary>
        /// Number of rows imported by table
        /// </summary>
        public Dictionary<string, int> RowsImportedByTable { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// List of warnings
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// List of errors
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}
