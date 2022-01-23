using Binner.Model.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Binner.Common.IO
{
    /// <summary>
    /// Exports data as SQL
    /// </summary>
    public sealed class SqlDataExporter : IDataExporter
    {
        public IDictionary<StreamName, Stream> Export(IBinnerDb db)
        {
            const string delimiter = ", ";
            var lineBreak = Environment.NewLine;
            var streams = new Dictionary<StreamName, Stream>();
            var builder = new DataSetBuilder();
            var dataSet = builder.Build(db);

            foreach (DataTable dataTable in dataSet.Tables)
            {
                var stream = new MemoryStream();
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        // add each column name
                        var columnNames = new List<string>();
                        var columnValues = new List<string>();
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            columnNames.Add($@"""{col.ColumnName}""");
                            columnValues.Add(@$"{EscapeValue(row[col], col.DataType)}");
                        }
                        writer.Write($"INSERT INTO {dataTable.TableName} ({string.Join(delimiter, columnNames)}) VALUES({string.Join(delimiter, columnValues)});{lineBreak}");
                    }
                }
                stream.Seek(0, SeekOrigin.Begin);
                streams.Add(new StreamName(dataTable.TableName, "sql"), stream);
            }
            return streams;
        }

        private string EscapeValue(object value, Type dataType)
        {
            if (dataType == typeof(string))
                return value == null ? "NULL" : $@"'{value?.ToString().Replace("'", "\'")}'";
            if (dataType == typeof(Guid))
                return value == null ? "NULL" : $@"'{value?.ToString().Replace("'", "\'")}'";
            if (dataType == typeof(ICollection<string>))
                return value == null ? "NULL" : $@"'{string.Join(",", value)}'";
            if (dataType == typeof(ICollection<string>))
                return value == null ? "NULL" : $@"{string.Join(" ", value)}";
            if (dataType == typeof(DateTime))
                return value == null ? "NULL" : $@"'{(DateTime)value:yyyy-MM-dd HH:mm:ss}'";
            return value.ToString();
        }
    }
}
