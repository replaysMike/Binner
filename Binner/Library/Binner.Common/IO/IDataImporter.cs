using Binner.Common.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public interface IDataImporter
    {
        /// <summary>
        /// Import Binner user data
        /// </summary>
        /// <param name="filename">Filename being imported</param>
        /// <param name="stream">The data to import</param>
        /// <param name="userContext">User context</param>
        Task<ImportResult> ImportAsync(string filename, Stream stream, UserContext userContext);

        /// <summary>
        /// Import Binner user data
        /// </summary>
        /// <param name="files">Collection of files to import</param>
        /// <param name="userContext">User context</param>
        Task<ImportResult> ImportAsync(IEnumerable<UploadFile> files, UserContext userContext);
    }
}
