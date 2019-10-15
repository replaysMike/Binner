using Binner.Common.StorageProviders;
using System.Collections.Generic;
using System.IO;

namespace Binner.Common.IO
{
    public interface IDataExporter
    {
        /// <summary>
        /// Export data to a stream
        /// </summary>
        /// <param name="db">The database to export</param>
        IDictionary<string, Stream> Export(IBinnerDb db);
    }
}
