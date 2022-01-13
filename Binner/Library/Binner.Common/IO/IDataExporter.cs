using Binner.Model.Common;
using System.Collections.Generic;
using System.IO;

namespace Binner.Common.IO
{
    public interface IDataExporter
    {
        /// <summary>
        /// Export Binner database to multiple streams, one for each table.
        /// </summary>
        /// <param name="db">The Binner database to export</param>
        IDictionary<StreamName, Stream> Export(IBinnerDb db);
    }
}
