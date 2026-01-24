using Binner.Model.Responses;
using NPOI.SS.Formula.Functions;
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
            var lineNumber = 0;

            // All parts
            var allHeaderValues = new List<string>
            {
                "Pcb",
                "PartNumber",
                "Mfr Part",
                "Mfr",
                "Part Type",
                "Cost",
                "TotalCost",
                "Currency",
                "Qty Required",
                "Qty In Stock",
                "Lead Time",
                "Reference Id",
                "Description",
                "Note",
                "SchematicReferenceId",
                "CustomDescription",
                "Package",
                "Mounting Type",
                "DatasheetUrl",
                "ProductUrl",
                "Location",
                "BinNumber",
                "BinNumber2",
                "ExtensionValue1",
                "ExtensionValue2",
                "DigiKey Part",
                "Mouser Part",
                "Arrow Part",
                "Tme Part",
                "Element14 Part",
                "Footprint Name",
                "Symbol Name",
                "Keywords",
                "Custom",
                "Product Status",
                "Base Product Number",
                "Series",
                "Rohs Status",
            };

            var allPartsStream = new MemoryStream();
            using var allPartsWriter = new StreamWriter(allPartsStream, Encoding.UTF8, 4096, true);
            // write the csv header
            allPartsWriter.Write($"#{string.Join(delimiter, allHeaderValues)}{lineBreak}");

            var rows = data.Parts
                .OrderBy(x => x.PcbId)
                .ThenBy(x => x.PartName)
                .ToList();
            foreach (var part in rows)
            {
                var rowValues = new List<string>
                {
                    $"{EscapeValue(data.Pcbs.Where(x => x.PcbId == part.PcbId).Select(x => x.Name).FirstOrDefault(), typeof(string))}",
                    $"{EscapeValue(part.PartName, typeof(string))}",
                    $"{EscapeValue(part.Part?.ManufacturerPartNumber, typeof(string))}",
                    $"{EscapeValue(part.Part?.Manufacturer, typeof(string))}",
                    $"{EscapeValue(part.Part?.PartType, typeof(string))}",
                    $"{part.Part?.Cost ?? (decimal)part.Cost}",
                    $"{(part.Part?.Cost ?? (decimal)part.Cost) * part.Quantity}", // total cost
                    $"{EscapeValue(part.Part?.Currency?? part.Currency, typeof(string))}",
                    $"{part.Quantity}",
                    $"{part.Part?.Quantity ?? part.QuantityAvailable}",
                    $"{EscapeValue(part.LeadTime ?? part.Part?.LeadTime, typeof(string))}",
                    $"{EscapeValue(part.ReferenceId, typeof(string))}",
                    $"{EscapeValue(part.Part?.Description, typeof(string))}",
                    $"{EscapeValue(part.Notes, typeof(string))}",
                    $"{EscapeValue(part.SchematicReferenceId, typeof(string))}",
                    $"{EscapeValue(part.CustomDescription, typeof(string))}",
                    $"{EscapeValue(part.Part?.PackageType, typeof(string))}",
                    $"{EscapeValue(part.Part?.MountingType, typeof(string))}",
                    $"{EscapeValue(part.Part?.DatasheetUrl, typeof(string))}",
                    $"{EscapeValue(part.Part?.ProductUrl, typeof(string))}",
                    $"{EscapeValue(part.Part?.Location, typeof(string))}",
                    $"{EscapeValue(part.Part?.BinNumber, typeof(string))}",
                    $"{EscapeValue(part.Part?.BinNumber2, typeof(string))}",
                    $"{EscapeValue(part.Part?.ExtensionValue1, typeof(string))}",
                    $"{EscapeValue(part.Part?.ExtensionValue2, typeof(string))}",
                    $"{EscapeValue(part.Part?.DigiKeyPartNumber, typeof(string))}",
                    $"{EscapeValue(part.Part?.MouserPartNumber, typeof(string))}",
                    $"{EscapeValue(part.Part?.ArrowPartNumber, typeof(string))}",
                    $"{EscapeValue(part.Part?.TmePartNumber, typeof(string))}",
                    $"{EscapeValue(part.Part?.Element14PartNumber, typeof(string))}",
                    $"{EscapeValue(part.Part?.FootprintName, typeof(string))}",
                    $"{EscapeValue(part.Part?.SymbolName, typeof(string))}",
                    $"{EscapeValue(part.Part?.Keywords, typeof(string))}",
                    $"{EscapeValue(string.Join(", ", part.Part?.CustomFields.Select(x => $"{x.Field}={x.Value}").ToList() ?? new List<string>()), typeof(string))}",
                    $"{EscapeValue(part.Part?.ProductStatus, typeof(string))}",
                    $"{EscapeValue(part.Part?.BaseProductNumber, typeof(string))}",
                    $"{EscapeValue(part.Part?.Series, typeof(string))}",
                    $"{EscapeValue(part.Part?.RohsStatus, typeof(string))}",
                };

                if (lineNumber < rows.Count - 1)
                    allPartsWriter.Write($"{string.Join(delimiter, rowValues)}{lineBreak}");
                else
                    allPartsWriter.Write($"{string.Join(delimiter, rowValues)}"); // no line break on last record as per csv standards
                lineNumber++;
            }
            //var test = new StreamReader(allPartsStream).ReadToEnd();
            streams.Add(new StreamName("AllParts", "csv"), allPartsStream);

            // Unassociated parts
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
                    "Total Cost",
                    "Currency",
                    "Qty Required",
                    "Qty In Stock",
                    "Lead Time",
                    "Reference Id",
                    "Description",
                    "Note",
                    "SchematicReferenceId",
                    "CustomDescription",
                };

                var unassociatedPartsStream = new MemoryStream();
                using var unassociatedPartsWriter = new StreamWriter(unassociatedPartsStream, Encoding.UTF8, 4096, true);
                // write the csv header
                unassociatedPartsWriter.Write($"#{string.Join(delimiter, headerValues)}{lineBreak}");
                lineNumber = 0;
                rows = data.Parts
                    .Where(x => x.PcbId == null)
                    .OrderBy(x => x.PartName)
                    .ToList();
                foreach (var part in rows)
                {
                    var outOfStock = part.Quantity > (part.Part?.Quantity ?? part.QuantityAvailable);
                    var rowValues = new List<string>
                    {
                        $"{(outOfStock ? 1 : 0)}",
                        $"{EscapeValue(part.PartName, typeof(string))}",
                        $"{EscapeValue(part.Part?.ManufacturerPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.PartType, typeof(string))}",
                        $"{part.Part?.Cost ?? (decimal)part.Cost}",
                        $"{(part.Part?.Cost ?? (decimal)part.Cost) * part.Quantity}",
                        $"{part.Part?.Currency?? part.Currency}",
                        $"{part.Quantity}",
                        $"{part.Part?.Quantity ?? part.QuantityAvailable}",
                        $"{EscapeValue("", typeof(string))}", // lead time
                        $"{EscapeValue(part.ReferenceId, typeof(string))}",
                        $"{EscapeValue(part.Part?.Description, typeof(string))}",
                        $"{EscapeValue(part.Notes, typeof(string))}",
                        $"{EscapeValue(part.SchematicReferenceId, typeof(string))}",
                        $"{EscapeValue(part.CustomDescription, typeof(string))}",
                    };

                    if (lineNumber < rows.Count - 1)
                        unassociatedPartsWriter.Write($"{string.Join(delimiter, rowValues)}{lineBreak}");
                    else
                        unassociatedPartsWriter.Write($"{string.Join(delimiter, rowValues)}"); // no line break on last record as per csv standards
                    lineNumber++;
                }
                streams.Add(new StreamName("Unassociated", "csv"), unassociatedPartsStream);
            }

            // By PCB
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
                    "Total Cost",
                    "Currency",
                    "Qty Required",
                    "Qty In Stock",
                    "Lead Time",
                    "Reference Id",
                    "Description",
                    "Note",
                    "SchematicReferenceId",
                    "CustomDescription",
                    "Package",
                    "Mounting Type",
                    "DatasheetUrl",
                    "ProductUrl",
                    "Location",
                    "BinNumber",
                    "BinNumber2",
                    "ExtensionValue1",
                    "ExtensionValue2",
                    "DigiKey Part",
                    "Mouser Part",
                    "Arrow Part",
                    "Tme Part",
                    "Element14 Part",
                    "Footprint Name",
                    "Symbol Name",
                    "Keywords",
                    "Custom",
                    "Product Status",
                    "Base Product Number",
                    "Series",
                    "Rohs Status",
                };

                var pcbPartsStream = new MemoryStream();
                using var pcbWriter = new StreamWriter(pcbPartsStream, Encoding.UTF8, 4096, true);
                // write the csv header
                pcbWriter.Write($"#{string.Join(delimiter, headerValues)}{lineBreak}");
                lineNumber = 0;
                rows = data.Parts
                    .Where(x => x.PcbId == pcb.PcbId)
                    .OrderBy(x => x.PartName)
                    .ToList();
                foreach (var part in rows)
                {
                    var outOfStock = part.Quantity > (part.Part?.Quantity ?? part.QuantityAvailable);
                    var rowValues = new List<string>
                    {
                        $"{(outOfStock ? 1 : 0)}",
                        $"{EscapeValue(part.PartName, typeof(string))}",
                        $"{EscapeValue(part.Part?.ManufacturerPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.PartType, typeof(string))}",
                        $"{part.Part?.Cost ?? (decimal)part.Cost}",
                        $"{(part.Part?.Cost ?? (decimal)part.Cost) * part.Quantity}",
                        $"{part.Part?.Currency?? part.Currency}",
                        $"{part.Quantity}",
                        $"{part.Part?.Quantity ?? part.QuantityAvailable}",
                        $"{EscapeValue("", typeof(string))}", // lead time
                        $"{EscapeValue(part.ReferenceId, typeof(string))}",
                        $"{EscapeValue(part.Part?.Description, typeof(string))}",
                        $"{EscapeValue(part.Notes, typeof(string))}",
                        $"{EscapeValue(part.SchematicReferenceId, typeof(string))}",
                        $"{EscapeValue(part.CustomDescription, typeof(string))}",
                        $"{EscapeValue(part.Part?.PackageType, typeof(string))}",
                        $"{EscapeValue(part.Part?.MountingType, typeof(string))}",
                        $"{EscapeValue(part.Part?.DatasheetUrl, typeof(string))}",
                        $"{EscapeValue(part.Part?.ProductUrl, typeof(string))}",
                        $"{EscapeValue(part.Part?.Location, typeof(string))}",
                        $"{EscapeValue(part.Part?.BinNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.BinNumber2, typeof(string))}",
                        $"{EscapeValue(part.Part?.ExtensionValue1, typeof(string))}",
                        $"{EscapeValue(part.Part?.ExtensionValue2, typeof(string))}",
                        $"{EscapeValue(part.Part?.DigiKeyPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.MouserPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.ArrowPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.TmePartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.Element14PartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.FootprintName, typeof(string))}",
                        $"{EscapeValue(part.Part?.SymbolName, typeof(string))}",
                        $"{EscapeValue(part.Part?.Keywords, typeof(string))}",
                        $"{EscapeValue(string.Join(", ", part.Part?.CustomFields.Select(x => $"{x.Field}={x.Value}").ToList() ?? new List<string>()), typeof(string))}",
                        $"{EscapeValue(part.Part?.ProductStatus, typeof(string))}",
                        $"{EscapeValue(part.Part?.BaseProductNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.Series, typeof(string))}",
                        $"{EscapeValue(part.Part?.RohsStatus, typeof(string))}",
                    };

                    if (lineNumber < rows.Count - 1)
                        pcbWriter.Write($"{string.Join(delimiter, rowValues)}{lineBreak}");
                    else
                        pcbWriter.Write($"{string.Join(delimiter, rowValues)}"); // no line break on last record as per csv standards
                    lineNumber++;
                }

                streams.Add(new StreamName(pcb.Name ?? $"Pcb {pcbNum}", "csv"), pcbPartsStream);
            }

            // Out of Stock parts
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
                    "Total Cost",
                    "Currency",
                    "Qty Required",
                    "Qty In Stock",
                    "Lead Time",
                    "Reference Id",
                    "Description",
                    "Note",
                    "SchematicReferenceId",
                    "CustomDescription",
                    "Package",
                    "Mounting Type",
                    "DatasheetUrl",
                    "ProductUrl",
                    "Location",
                    "BinNumber",
                    "BinNumber2",
                    "ExtensionValue1",
                    "ExtensionValue2",
                    "DigiKey Part",
                    "Mouser Part",
                    "Arrow Part",
                    "Tme Part",
                    "Element14 Part",
                    "Footprint Name",
                    "Symbol Name",
                    "Keywords",
                    "Custom",
                    "Product Status",
                    "Base Product Number",
                    "Series",
                    "Rohs Status",
                };

                var stream = new MemoryStream();
                using var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true);
                // write the csv header
                writer.Write($"#{string.Join(delimiter, headerValues)}{lineBreak}");

                lineNumber = 0;
                rows = data.Parts
                    .Where(x => x.Quantity > (x.Part?.Quantity ?? x.QuantityAvailable))
                    .OrderBy(x => x.PcbId)
                    .ThenBy(x => x.PartName)
                    .ToList();
                foreach (var part in rows)
                {
                    var rowValues = new List<string>
                    {
                        $"{EscapeValue(data.Pcbs.Where(x => x.PcbId == part.PcbId).Select(x => x.Name).FirstOrDefault(), typeof(string))}",
                        $"{EscapeValue(part.PartName, typeof(string))}",
                        $"{EscapeValue(part.Part?.ManufacturerPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.PartType, typeof(string))}",
                        $"{part.Part?.Cost ?? (decimal)part.Cost}",
                        $"{(part.Part?.Cost ?? (decimal)part.Cost) * part.Quantity}",
                        $"{part.Part?.Currency?? part.Currency}",
                        $"{part.Quantity}",
                        $"{part.Part?.Quantity ?? part.QuantityAvailable}",
                        $"{EscapeValue("", typeof(string))}", // lead time
                        $"{EscapeValue(part.ReferenceId, typeof(string))}",
                        $"{EscapeValue(part.Part?.Description, typeof(string))}",
                        $"{EscapeValue(part.Notes, typeof(string))}",
                        $"{EscapeValue(part.SchematicReferenceId, typeof(string))}",
                        $"{EscapeValue(part.CustomDescription, typeof(string))}",
                        $"{EscapeValue(part.Part?.MountingType, typeof(string))}",
                        $"{EscapeValue(part.Part?.DatasheetUrl, typeof(string))}",
                        $"{EscapeValue(part.Part?.ProductUrl, typeof(string))}",
                        $"{EscapeValue(part.Part?.Location, typeof(string))}",
                        $"{EscapeValue(part.Part?.BinNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.BinNumber2, typeof(string))}",
                        $"{EscapeValue(part.Part?.ExtensionValue1, typeof(string))}",
                        $"{EscapeValue(part.Part?.ExtensionValue2, typeof(string))}",
                        $"{EscapeValue(part.Part?.DigiKeyPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.MouserPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.ArrowPartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.TmePartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.Element14PartNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.FootprintName, typeof(string))}",
                        $"{EscapeValue(part.Part?.SymbolName, typeof(string))}",
                        $"{EscapeValue(part.Part?.Keywords, typeof(string))}",
                        $"{EscapeValue(string.Join(", ", part.Part?.CustomFields.Select(x => $"{x.Field}={x.Value}").ToList() ?? new List<string>()), typeof(string))}",
                        $"{EscapeValue(part.Part?.ProductStatus, typeof(string))}",
                        $"{EscapeValue(part.Part?.BaseProductNumber, typeof(string))}",
                        $"{EscapeValue(part.Part?.Series, typeof(string))}",
                        $"{EscapeValue(part.Part?.RohsStatus, typeof(string))}",
                    };

                    if (lineNumber < rows.Count - 1)
                        writer.Write($"{string.Join(delimiter, rowValues)}{lineBreak}");
                    else
                        writer.Write($"{string.Join(delimiter, rowValues)}"); // no line break on last record as per csv standards
                    lineNumber++;
                }
                streams.Add(new StreamName("OutOfStock", "csv"), stream);
            }

            return streams;
        }

        private string EscapeValue(object? value, Type dataType)
        {
            if (Options.HasFlag(CsvOptions.QuoteStrings))
            {
                if (value == null) return @""""""; // return empty quoted string
                if (dataType == typeof(string))
                    return $@"""{value?.ToString()}""";
                if (dataType == typeof(ICollection<string>))
                    return $@"""{string.Join(",", value)}""";
            }
            if (value == null) return string.Empty; // return empty string
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
