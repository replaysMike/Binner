using Binner.Global.Common;
using Binner.Model;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Imports data from Excel Open XML Format 2007+ (XLSX)
    /// </summary>
    public class ExcelDataImporter : BaseDataImporter
    {
        // SupportedTables ordering matters when it comes to relational data!
        private readonly string[] SupportedTables = ["Projects", "PartTypes", "Parts", "PartParametrics", "PartModels", "CustomFields", "CustomFieldValues", "Pcbs", "ProjectPcbAssignments", "ProjectPartAssignments"];
        private readonly IStorageProvider _storageProvider;

        public ExcelDataImporter(IStorageProvider storageProvider) : base(storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public override async Task<ImportResult> ImportAsync(IEnumerable<UploadFile> files, IUserContext? userContext)
        {
            var result = new ImportResult();
            foreach (var file in files)
            {
                var fileResult = await ImportAsync(file.Filename, file.Stream, userContext);
                result.TotalRowsImported += fileResult.TotalRowsImported;
                result.Success |= fileResult.Success;
                result.Warnings.AddRange(fileResult.Warnings);
                result.Errors.AddRange(fileResult.Errors);
                foreach (var tableTotal in fileResult.RowsImportedByTable)
                {
                    if (!result.RowsImportedByTable.ContainsKey(tableTotal.Key))
                        result.RowsImportedByTable.Add(tableTotal.Key, 0);
                    result.RowsImportedByTable[tableTotal.Key] += tableTotal.Value;
                }
            }
            return result;
        }

        public override async Task<ImportResult> ImportAsync(string filename, Stream stream, IUserContext? userContext)
        {
            var result = new ImportResult();
            foreach (var table in SupportedTables)
                result.RowsImportedByTable.Add(table, 0);
            // get the global part types, and the user's custom part types
            var partTypes = (await _storageProvider.GetPartTypesAsync(userContext)).ToList();
            try
            {
                stream.Position = 0;
                var workbook = WorkbookFactory.Create(stream);
                foreach (var tableName in SupportedTables)
                {
                    var worksheet = workbook.GetSheet(tableName);
                    if (worksheet != null)
                    {
                        // parse worksheet
                        var header = new Header(worksheet.GetRow(0));
                        for (var rowNumber = 1; rowNumber <= worksheet.LastRowNum; rowNumber++)
                        {
                            var rowData = worksheet.GetRow(rowNumber);
                            if (rowData == null) continue;
                            await AddRowAsync(result, rowNumber, tableName, rowData, header, partTypes, userContext);
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

        private async Task AddRowAsync(ImportResult result, int rowNumber, string? tableName, IRow rowData, Header header, ICollection<PartType> partTypes, IUserContext? userContext)
        {
            if (string.IsNullOrEmpty(tableName))
                return;

            switch (tableName.ToLower())
            {
                case "projects":
                    {
                        // import project info
                        var (values, errors) = MapObject<Project>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddProjectAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "parttypes":
                    {
                        // import partTypes info
                        var (values, errors) = MapObject<PartType>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddPartTypeAsync(rowNumber, values, partTypes, result, userContext);
                    }
                    break;
                case "parts":
                    {
                        // import parts info
                        var (values, errors) = MapObject<Part>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddPartAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "partmodels":
                    {
                        // import part models
                        var (values, errors) = MapObject<PartModel>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddPartModelsAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "partparametrics":
                    {
                        // import part parametrics
                        var (values, errors) = MapObject<PartParametric>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddPartParametricsAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "customfields":
                    {
                        // import custom fields
                        var (values, errors) = MapObject<CustomField>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddCustomFieldsAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "customfieldvalues":
                    {
                        // import custom fields
                        var (values, errors) = MapObject<CustomFieldValue>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddCustomFieldValuesAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "pcbs":
                    {
                        // import project pcb's
                        var (values, errors) = MapObject<Pcb>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddPcbAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "projectpcbassignments":
                    {
                        // import project pcb assignments
                        var (values, errors) = MapObject<ProjectPcbAssignment>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddProjectPcbAssignmentAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "projectpartassignments":
                    {
                        // import project part assignments
                        var (values, errors) = MapObject<ProjectPartAssignment>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{rowData.ToString()}'");
                            return;
                        }

                        await AddProjectPartAssignmentAsync(rowNumber, values, result, userContext);
                    }
                    break;
            }
        }

        private (Dictionary<string, object?> Values, List<string> Errors) MapObject<T>(IRow rowData, Header header)
        {
            var values = new Dictionary<string, object?>();
            var errors = new List<string>();

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var @switch = new Dictionary<Type, Action> {
                    { typeof(long), () => Map<long>(property, rowData, header, ref values, ref errors) },
                    { typeof(long?), () => Map<long?>(property, rowData, header, ref values, ref errors) },
                    { typeof(string), () => Map<string>(property, rowData, header, ref values, ref errors) },
                    { typeof(double), () => Map<double>(property, rowData, header, ref values, ref errors) },
                    { typeof(decimal), () => Map<decimal>(property, rowData, header, ref values, ref errors) },
                    { typeof(bool), () => Map<bool>(property, rowData, header, ref values, ref errors) },
                    { typeof(int), () => Map<int>(property, rowData, header, ref values, ref errors) },
                    { typeof(DateTime), () => Map<DateTime>(property, rowData, header, ref values, ref errors, DateTime.UtcNow) },
                            };

                var propertyType = property.PropertyType;
                if (@switch.ContainsKey(propertyType))
                    @switch[propertyType]();
            }
            return (values, errors);
        }

        private void Map<T>(PropertyInfo property, IRow rowData, Header header, ref Dictionary<string, object?> values, ref List<string> errors, object? defaultValue = null)
        {
            var isSuccess = TryGet<T>(rowData, header, property.Name, out var value);
            MapValue(isSuccess, value, property, ref values, ref errors, defaultValue);
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

            return TryGetQuotedValue(unquotedValue, out value);
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
