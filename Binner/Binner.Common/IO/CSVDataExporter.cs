using Binner.Common.StorageProviders;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Binner.Common.IO
{
    public class CSVDataExporter : IDataExporter
    {
        public IDictionary<string, Stream> Export(IBinnerDb db)
        {
            const string delimiter = ",";
            const string lineBreak = "\r\n";
            var streams = new Dictionary<string, Stream>();
            var builder = new DataSetBuilder();
            var dataSet = builder.Build(db);

            foreach (DataTable dataTable in dataSet.Tables)
            {
                var stream = new MemoryStream();
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
                {
                    foreach (DataRow row in dataTable.Rows) {
                        var rowValues = new List<string>();
                        foreach (DataColumn col in dataTable.Columns) {
                            rowValues.Add($"{EscapeValue(row[col], col.DataType)}");
                        }
                        writer.Write($"{string.Join(delimiter, rowValues)}{lineBreak}");
                    }
                }
                streams.Add(dataTable.TableName, stream);
            }
            return streams;
        }

        private string EscapeValue(object value, Type dataType)
        {
            if (dataType == typeof(string))
                return $@"""{value?.ToString()}""";
            return value.ToString();
        }
    }
}
