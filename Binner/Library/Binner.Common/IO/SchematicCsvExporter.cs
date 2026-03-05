using Binner.Model.Swarm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Export a schematic (circuit) to a csv file
    /// </summary>
    public class SchematicCsvExporter
    {
        public CsvOptions Options => CsvOptions.QuoteStrings;

        public SchematicCsvExporter()
        {

        }

        public async Task<SchematicExportResult> ExportAsync(Circuit circuit)
        {
            if (circuit == null) return new SchematicExportResult { ErrorMessage = "Schematic cannot be null" };
            if (circuit.Parts == null || !circuit.Parts.Any()) return new SchematicExportResult { ErrorMessage = "Schematic has no parts" };
            const string delimiter = ",";
            var lineBreak = Environment.NewLine;
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            {
                var headerValues = new List<string>() { "Reference", "Part", "Type", "Description" };
                writer.Write($"#{string.Join(delimiter, headerValues)}{lineBreak}");

                foreach(var part in circuit.Parts)
                {
                    writer.Write($"{EscapeValue(part.Reference)}{delimiter}{EscapeValue(part.PartName ?? part.PartNumberManufacturer?.Name ?? part.PartNumber?.Name ?? "Unknown")}{delimiter}{EscapeValue(part.PartType)}{delimiter}{EscapeValue(part.Description)}{lineBreak}");
                }
            }
            return new SchematicExportResult
            {
                Stream = stream,
                Circuit = circuit,
            };
        }

        private string EscapeValue(object? value, Type? dataType = null)
        {
            if (dataType == null) dataType = typeof(string);
            if (Options.HasFlag(CsvOptions.QuoteStrings))
            {
                if (value == null) return @""""""; // return empty quoted string

                // quote string collections as comma-separated lists
                if (dataType == typeof(ICollection<string>))
                    return $@"""{string.Join(",", value).Replace("\"", "\"\"")}""";

                // quote all other types (strings, numbers (because of european formats), etc)
                return $@"""{value?.ToString().Replace("\"", "\"\"")}""";
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

    public class SchematicExportResult
    {
        public Stream? Stream { get; set; }
        public Circuit? Circuit { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
