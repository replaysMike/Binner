using Binner.Global.Common;
using Binner.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public class CsvBOMImporter
    {
        // SupportedTables ordering matters when it comes to relational data!
        private readonly IStorageProvider _storageProvider;
        private readonly TemporaryKeyTracker _temporaryKeyTracker = new TemporaryKeyTracker();

        public CsvBOMImporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        private string? GetValueFromHeader(string[] rowData, Header header, string name)
        {
            var headerIndex = header.GetHeaderIndex(name);
            if (headerIndex >= 0)
                return rowData[headerIndex];
            return null;
        }

        public async Task<ImportResult> ImportAsync(Project project, Stream stream, IUserContext? userContext)
        {
            var result = new ImportResult();
            try
            {
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var data = await reader.ReadToEndAsync();
                // remove line breaks
                data = data.Replace("\r", "");

                var rows = SplitBoundaries(data, new char[] { '\n' });
                if (!rows.Any())
                {
                    result.Success = true;
                    result.Warnings.Add("No rows were found!");
                    return result;
                }

                // read csv header
                Header? header = null;
                var headerRow = rows.First();
                if (headerRow.StartsWith("\"#\""))
                {
                    header = new Header(headerRow);
                }
                else
                {
                    result.Success = false;
                    result.Errors.Add("No CSV row header was found! A CSV row header is required as the first row of data to indicate the column ordering. Example: '#Name,Description,Location,Color,DateCreatedUtc,DateModifiedUtc'");
                    return result;
                }
                var rowNumber = 0;
                foreach (var row in rows)
                {
                    // skip the header row
                    if (rowNumber == 0)
                    {
                        rowNumber++;
                        continue;
                    }
                    var rowData = SplitBoundaries(row, new char[] { ',' }, true);
                    if (rowData.Length != header.Headers.Count)
                    {
                        result.Warnings.Add($"[Row {rowNumber}] Row does not contain the same number of columns as the header, skipping...");
                        continue;
                    }

                    // import BOM info
                    var isPartNumberValid = TryGet<string?>(rowData, header, "MPN", out var partNumber);
                    var isQuantityValid = TryGet<int>(rowData, header, "Qty", out var quantity);
                    var isReferenceValid = TryGet<string?>(rowData, header, "Reference", out var reference);
                    var isNoteValid = TryGet<string?>(rowData, header, "Value", out var note);

                    if (!isPartNumberValid || !isQuantityValid || !isReferenceValid)
                        continue;

                    ProjectPartAssignment assignment = new ProjectPartAssignment();
                    assignment.ProjectId = project.ProjectId;
                    assignment.Quantity = quantity;
                    assignment.Notes = note;
                    assignment.SchematicReferenceId = reference;

                    var part = await _storageProvider.GetPartAsync(partNumber, userContext);
                    if (part != null)
                    {
                        assignment.PartName = part.PartNumber;
                        assignment.PartId   = part.PartId;
                    }
                    else
                    {
                        part = new Part();
                        part.PartNumber = partNumber;
                        part.Quantity = 0;
                        part.UserId = userContext?.UserId;
                        part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Other;
                        try
                        {
                            part = await _storageProvider.AddPartAsync(part, userContext);
                        }
                        catch (Exception ex)
                        {
                            // failed to add part
                            result.Errors.Add($"[Row {rowNumber}'] Part with PartNumber '{partNumber}' could not be added. Error: {ex.Message}");
                        }

                        assignment.PartName = partNumber;
                        assignment.PartId   = part.PartId;
                    }

                    try
                    {
                        await _storageProvider.AddProjectPartAssignmentAsync(assignment, userContext);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"[Row {rowNumber}] BOM entry '{partNumber}' could not be added. Error: {ex.Message}");
                    }

                    rowNumber++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            result.Success = !result.Errors.Any();
            return result;
        }

        private bool TryGet<T>(string?[] rowData, Header header, string name, out T? value)
        {
            value = default;
            var type = typeof(T);
            var columnIndex = header.GetHeaderIndex(name);
            if (columnIndex < 0 || columnIndex >= rowData.Length)
            {
                value = default;
                return true;
            }

            if (Nullable.GetUnderlyingType(type) != null && rowData[columnIndex] == null)
                return true;
            var unquotedValue = GetQuoted(rowData[columnIndex]);

            if (type == typeof(string))
            {
                if (!string.IsNullOrEmpty(unquotedValue))
                    value = (T)(object)unquotedValue;
                return true;
            }
            if (type == typeof(long) || type == typeof(long?))
            {
                var isLongValid = long.TryParse(unquotedValue, out var longValue);
                value = (T)(object)longValue;
                return isLongValid;
            }
            if (type == typeof(int) || type == typeof(int?))
            {
                var isIntValid = int.TryParse(unquotedValue, out var intValue);
                value = (T)(object)intValue;
                return isIntValid;
            }
            if (type == typeof(bool) || type == typeof(bool?))
            {
                var isIntValid = bool.TryParse(unquotedValue, out var boolValue);
                value = (T)(object)boolValue;
                return isIntValid;
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                var isDateValid = DateTime.TryParse(unquotedValue, out var dateValue);
                value = (T)(object)dateValue;
                return isDateValid;
            }
            if (type == typeof(double) || type == typeof(double?))
            {
                var isDoubleValid = double.TryParse(unquotedValue, out var doubleValue);
                value = (T)(object)doubleValue;
                return isDoubleValid;
            }
            return false;
        }

        private string? GetQuoted(string? val)
        {
            if (val == null)
                return null;
            var match = Regex.Match(val, @"'[^']*'|\""[^\""]*\""");
            if (match.Success)
            {
                var value = match.Value.Substring(1, match.Value.Length - 2);
                // decode line breaks
                value = value.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
                return value;
            }
            // decode line breaks
            val = val.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
            return val;
        }

        private string[] SplitBoundaries(string data, char[] rowDelimiters, bool removeBoundary = false)
        {
            var rows = new List<string>();
            var quotes = new List<char> { '"', '\'' };
            var startPos = 0;
            var insideQuotes = false;
            var insideQuotesChar = '\0';
            for (var i = 0; i < data.Length; i++)
            {
                var c = data[i];
                if (quotes.Contains(c))
                {
                    if (!insideQuotes)
                    {
                        insideQuotes = true;
                        insideQuotesChar = c;
                    }
                    else if (c == insideQuotesChar)
                    {
                        insideQuotes = false;
                    }
                }
                if ((rowDelimiters.Any(x => x.Equals(c)) && !insideQuotes) || i == data.Length - 1)
                {
                    var row = data.Substring(startPos, i - startPos + 1 - (removeBoundary && !(i == data.Length - 1) ? 1 : 0));
                    if (!string.IsNullOrEmpty(row) && row.Length > (removeBoundary ? 0 : 1))
                        rows.Add(row);
                    startPos = i + 1;
                }
            }

            return rows.ToArray();
        }

        public class Header
        {
            public List<HeaderIndex> Headers { get; set; } = new List<HeaderIndex>();

            public Header(string headerRow)
            {
                var headers = headerRow.Split(new string[] { "," }, StringSplitOptions.TrimEntries);
                for (var i = 0; i < headers.Length; i++)
                {
                    var name = headers[i].Replace("'", "").Replace("\"", "");
                    if (name.StartsWith("#"))
                        name = name.Substring(1);
                    Headers.Add(new HeaderIndex(name, i));
                }
            }

            public int GetHeaderIndex(string name)
            {
                var header = Headers.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (header == null)
                    return -1;
                return header.Index;
            }
        }

        public class HeaderIndex
        {
            public string Name { get; set; }
            public int Index { get; set; }

            public HeaderIndex(string name, int index)
            {
                Name = name;
                Index = index;
            }

            public override string ToString()
                => Name;
        }
    }
}
