using Binner.Model;
using NPOI.SS.Formula.Functions;
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
    public sealed class CsvDataExporter : IDataExporter
    {
        public CsvOptions Options => CsvOptions.QuoteStrings;

        public IDictionary<StreamName, Stream> Export(IBinnerDb db)
        {
            const string delimiter = ",";
            var lineBreak = Environment.NewLine;
            var streams = new Dictionary<StreamName, Stream>();
            var builder = new DataSetBuilder();
            var dataSet = builder.Build(db);

            foreach (DataTable dataTable in dataSet.Tables)
            {
                var stream = new MemoryStream();
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
                {
                    // write CSV header
                    var headerValues = new List<string>();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        headerValues.Add($"{col.ColumnName}");
                    }
                    writer.Write($"#{string.Join(delimiter, headerValues)}{lineBreak}");

                    // write data
                    var lineNumber = 0;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var rowValues = new List<string>();
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            rowValues.Add($"{EscapeValue(row[col], col.DataType)}");
                        }
                        if (lineNumber < dataTable.Rows.Count - 1)
                            writer.Write($"{string.Join(delimiter, rowValues)}{lineBreak}");
                        else
                            writer.Write($"{string.Join(delimiter, rowValues)}"); // no line break on last record as per csv standards
                        lineNumber++;
                    }
                }
                stream.Seek(0, SeekOrigin.Begin);
                streams.Add(new StreamName(dataTable.TableName, "csv"), stream);
            }
            return streams;
        }

        private string EscapeValue(object value, Type dataType)
        {
            if (Options.HasFlag(CsvOptions.QuoteStrings))
            {
                if (value == null) return @""""""; // return empty quoted string
                if (dataType == typeof(string))
                    return $@"""{value?.ToString().Replace("\"", "\"\"")}""";
                if (dataType == typeof(ICollection<string>))
                    return $@"""{string.Join(",", value).Replace("\"", "\"\"")}""";
            }
            if (value == null) return string.Empty; // return empty string
            if (dataType == typeof(ICollection<string>))
                return $@"{string.Join(" ", value).Replace("\"", "\"\"")}";
            if (dataType == typeof(DateTime))
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            return value.ToString() ?? string.Empty;
        }

        [Flags]
        public enum CsvOptions
        {
            None = 0,
            QuoteStrings = 1,
        }
    }
}
