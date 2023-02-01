using Binner.Model.Common;
using System.Collections.Generic;

namespace Binner.Common.Models.Responses
{
    public class PartStoredFilesResponse : PartResponse
    {
        /// <summary>
        /// List of user uploaded files
        /// </summary>
        public ICollection<StoredFile> StoredFiles { get; set; }
    }
}
