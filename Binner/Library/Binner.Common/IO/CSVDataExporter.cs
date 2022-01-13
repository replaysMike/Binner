using Binner.Model.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Binner.Common.IO
{
    /// <summary>
    /// Export data to CSV (Comma delimited file)
    /// </summary>
    public class CSVDataExporter : IDataExporter
    {
        public CSVOptions Options { get; } = CSVOptions.QuoteStrings;

        public IDictionary<StreamName, Stream> Export(IBinnerDb db)
        {
            const string delimiter = ",";
            const string lineBreak = "\r\n";
            var streams = new Dictionary<StreamName, Stream>();
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
                stream.Seek(0, SeekOrigin.Begin);
                streams.Add(new StreamName(dataTable.TableName, "csv"), stream);
            }
            return streams;
        }

        private string EscapeValue(object value, Type dataType)
        {
            if (Options.HasFlag(CSVOptions.QuoteStrings))
            {
                if (dataType == typeof(string))
                    return $@"""{value?.ToString()}""";
                if (dataType == typeof(ICollection<string>))
                    return $@"""{string.Join(",", value)}""";
            }
            if (dataType == typeof(ICollection<string>))
                return $@"{string.Join(" ", value)}";
            if (dataType == typeof(DateTime))
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            return value.ToString();
        }

        [Flags]
        public enum CSVOptions
        {
            None = 0,
            QuoteStrings = 1,
        }
    }
}
