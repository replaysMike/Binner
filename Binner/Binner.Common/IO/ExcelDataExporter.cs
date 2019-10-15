using Binner.Common.StorageProviders;
using ExcelLibrary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Binner.Common.IO
{
    public class ExcelDataExporter : IDataExporter
    {
        public IDictionary<string, Stream> Export(IBinnerDb db)
        {
            var streams = new Dictionary<string, Stream>();
            var builder = new DataSetBuilder();
            var dataSet = builder.Build(db);

            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            {
                DataSetHelper.CreateWorkbook(stream, dataSet);
            }
            streams.Add("BinnerParts", stream);
            return streams;
        }
    }
}
