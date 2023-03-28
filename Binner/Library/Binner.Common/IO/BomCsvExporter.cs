using Binner.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Binner.Common.IO
{
    public class BomCsvExporter
    {
        public CsvOptions Options => CsvOptions.QuoteStrings;

        public IDictionary<StreamName, Stream> Export(BomResponse data)
        {
            const string delimiter = ",";
            var lineBreak = Environment.NewLine;

            var streams = new Dictionary<StreamName, Stream>();
            // first write the unassociated parts
            if (data.Parts.Any(x => x.PcbId == null))
            {
                // write CSV header
                var headerValues = new List<string>
                {
                    "OutOfStock",
                    "PartNumber",
                    "Mfr Part",
                    "Part Type",
                    "Cost",
                    "Qty Required",
                    "Qty In Stock",
                    "Lead Time",
                    "Reference Id",
                    "Description",
                    "Note",
                    "SchematicReferenceId",
                    "CustomDescription",
                };

                var stream = new MemoryStream();
                using var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true);
                writer.Write($"#{string.Join(delimiter, headerValues)}{lineBreak}");
                
                foreach (var part in data.Parts.Where(x => x.PcbId == null))
                {
                    var outOfStock = part.Quantity > (part.Part?.Quantity ?? part.QuantityAvailable);
                    var rowValues = new List<string>
                    {
                        $"{(outOfStock ? 1 : 0)}",
                        $"{EscapeValue(part.PartName, typeof(string))}",
                        $"{EscapeValue(part.Part?.ManufacturerPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.PartType, typeof(string))}",
                        $"{part.Part?.Cost}",
                        $"{part.Quantity}",
                        $"{part.Part?.Quantity ?? part.QuantityAvailable}",
                        $"{EscapeValue("", typeof(string))}", // lead time
                        $"{EscapeValue(part.ReferenceId, typeof(string))}",
                        $"{EscapeValue(part.Part?.Description, typeof(string))}",
                        $"{EscapeValue(part.Notes, typeof(string))}",
                        $"{EscapeValue(part.SchematicReferenceId, typeof(string))}",
                        $"{EscapeValue(part.CustomDescription, typeof(string))}",
                    };

                    writer.Write($"{string.Join(delimiter, rowValues)}{lineBreak}");
                }
                stream.Seek(0, SeekOrigin.Begin);
                streams.Add(new StreamName("Unassociated", "csv"), stream);
            }

            var pcbNum = 0;
            foreach (var pcb in data.Pcbs)
            {
                pcbNum++;
                // write CSV header
                var headerValues = new List<string>
                {
                    "OutOfStock",
                    "PartNumber",
                    "Mfr Part",
                    "Part Type",
                    "Cost",
                    "Qty Required",
                    "Qty In Stock",
                    "Lead Time",
                    "Reference Id",
                    "Description",
                    "Note",
                    "SchematicReferenceId",
                    "CustomDescription",
                };

                var pcbStream = new MemoryStream();
                using var pcbWriter = new StreamWriter(pcbStream, Encoding.UTF8, 4096, true);
                pcbWriter.Write($"#{string.Join(delimiter, headerValues)}{lineBreak}");

                foreach (var part in data.Parts.Where(x => x.PcbId == pcb.PcbId))
                {
                    var outOfStock = part.Quantity > (part.Part?.Quantity ?? part.QuantityAvailable);
                    var rowValues = new List<string>
                    {
                        $"{(outOfStock ? 1 : 0)}",
                        $"{EscapeValue(part.PartName, typeof(string))}",
                        $"{EscapeValue(part.Part?.ManufacturerPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.PartType, typeof(string))}",
                        $"{part.Part?.Cost}",
                        $"{part.Quantity}",
                        $"{part.Part?.Quantity ?? part.QuantityAvailable}",
                        $"{EscapeValue("", typeof(string))}", // lead time
                        $"{EscapeValue(part.ReferenceId, typeof(string))}",
                        $"{EscapeValue(part.Part?.Description, typeof(string))}",
                        $"{EscapeValue(part.Notes, typeof(string))}",
                        $"{EscapeValue(part.SchematicReferenceId, typeof(string))}",
                        $"{EscapeValue(part.CustomDescription, typeof(string))}",
                    };

                    pcbWriter.Write($"{string.Join(delimiter, rowValues)}{lineBreak}");
                }

                pcbStream.Seek(0, SeekOrigin.Begin);
                streams.Add(new StreamName(pcb.Name ?? $"Pcb {pcbNum}", "csv"), pcbStream);
            }

            // create a file for all out of stock items
            if (data.Parts.Any(x => x.Quantity > (x.Part?.Quantity ?? x.QuantityAvailable)))
            {
                // write CSV header
                var headerValues = new List<string>
                {
                    "Pcb",
                    "PartNumber",
                    "Mfr Part",
                    "Part Type",
                    "Cost",
                    "Qty Required",
                    "Qty In Stock",
                    "Lead Time",
                    "Reference Id",
                    "Description",
                    "Note",
                    "SchematicReferenceId",
                    "CustomDescription",
                };

                var stream = new MemoryStream();
                using var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true);
                writer.Write($"#{string.Join(delimiter, headerValues)}{lineBreak}");
                
                foreach (var part in data.Parts.Where(x => x.Quantity > (x.Part?.Quantity ?? x.QuantityAvailable)))
                {
                    var rowValues = new List<string>
                    {
                        $"{EscapeValue(data.Pcbs.Where(x => x.PcbId == part.PcbId).Select(x => x.Name).FirstOrDefault(), typeof(string))}",
                        $"{EscapeValue(part.PartName, typeof(string))}",
                        $"{EscapeValue(part.Part?.ManufacturerPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.PartType, typeof(string))}",
                        $"{part.Part?.Cost}",
                        $"{part.Quantity}",
                        $"{part.Part?.Quantity ?? part.QuantityAvailable}",
                        $"{EscapeValue("", typeof(string))}", // lead time
                        $"{EscapeValue(part.ReferenceId, typeof(string))}",
                        $"{EscapeValue(part.Part?.Description, typeof(string))}",
                        $"{EscapeValue(part.Notes, typeof(string))}",
                        $"{EscapeValue(part.SchematicReferenceId, typeof(string))}",
                        $"{EscapeValue(part.CustomDescription, typeof(string))}",
                    };

                    writer.Write($"{string.Join(delimiter, rowValues)}{lineBreak}");
                }
                stream.Seek(0, SeekOrigin.Begin);
                streams.Add(new StreamName("OutOfStock", "csv"), stream);
            }

            return streams;
        }

        private string EscapeValue(object? value, Type dataType)
        {
            if (value == null) return string.Empty;
            if (Options.HasFlag(CsvOptions.QuoteStrings))
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
