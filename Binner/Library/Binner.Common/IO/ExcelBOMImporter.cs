using Binner.Global.Common;
using Binner.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Imports data from Excel Open XML Format 2007+ (XLSX)
    /// </summary>
    public class ExcelBOMImporter
    {
        // SupportedTables ordering matters when it comes to relational data!
        private readonly IStorageProvider _storageProvider;

        public ExcelBOMImporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<ImportResult> ImportAsync(Project project, Stream stream, IUserContext? userContext)
        {
            var result = new ImportResult();
            try
            {
                stream.Position = 0;
                var workbook = WorkbookFactory.Create(stream);
                var worksheet = workbook.GetSheetAt(0);
                if (worksheet != null)
                {
                    // parse worksheet
                    var header = new Header(worksheet.GetRow(0));
                    for (var rowNumber = 1; rowNumber <= worksheet.LastRowNum; rowNumber++)
                    {
                        var rowData = worksheet.GetRow(rowNumber);
                        if (rowData == null)
                            continue;

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
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            result.Success = !result.Errors.Any();

            return result;
        }

        private bool TryGet<T>(IRow rowData, Header header, string name, out T? value)
        {
            value = default;
            var type = typeof(T);
            var columnIndex = header.GetHeaderIndex(name);
            var cellValue = columnIndex >= 0 ? rowData.GetCell(columnIndex) : null;
            if (Nullable.GetUnderlyingType(type) != null && cellValue?.ToString() == null)
                return true;
            var unquotedValue = GetQuoted(cellValue?.ToString());

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

        public class Header
        {
            public List<HeaderIndex> Headers { get; set; } = new List<HeaderIndex>();

            public Header(IRow headerRow)
            {
                for (var i = 0; i < headerRow.LastCellNum; i++)
                {
                    var headerName = headerRow.GetCell(i).StringCellValue;
                    var name = headerName.Replace("'", "").Replace("\"", "");
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
