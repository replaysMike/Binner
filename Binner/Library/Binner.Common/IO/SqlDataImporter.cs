using Binner.Global.Common;
using Binner.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Imports data from Sql Insert statements
    /// </summary>
    public class SqlDataImporter : BaseDataImporter
    {
        // SupportedTables ordering matters when it comes to relational data!
        private readonly string[] SupportedTables = ["Projects", "PartTypes", "Parts", "PartParametrics", "PartModels", "CustomFields", "CustomFieldValues", "Pcbs", "ProjectPcbAssignments", "ProjectPartAssignments"];
        private readonly IStorageProvider _storageProvider;

        public SqlDataImporter(IStorageProvider storageProvider) : base(storageProvider)
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
            const char delimiter = ',';
            var result = new ImportResult();
            foreach (var table in SupportedTables)
                result.RowsImportedByTable.Add(table, 0);
            // get the global part types, and the user's custom part types
            var partTypes = (await _storageProvider.GetPartTypesAsync(false, userContext)).ToList();
            try
            {
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var data = await reader.ReadToEndAsync();
                // remove line breaks if present
                data = data.Replace("\r", "");

                var rows = SplitBoundaries(data, ['\n', ';']);
                // validate rows
                var rowNumber = 0;
                foreach (var row in rows)
                {
                    var tableName = GetTableName(row);
                    if (!SupportedTables.Contains(tableName, StringComparer.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(tableName))
                    {
                        result.Errors.Add($"[Row {rowNumber}] Insert into table '{tableName}' not a supported table name! Table name must be one of the following: {string.Join(",", SupportedTables)} and may optionally contain the schema prefix 'dbo'.");
                        result.Success = false;
                        return result;
                    }
                    rowNumber++;
                }

                // rows will be ordered in declaration order of SupportedTables
                rows = ReOrderInserts(rows);

                if (!rows.Any())
                {
                    result.Success = true;
                    result.Warnings.Add("No rows were found!");
                    return result;
                }

                rowNumber = 0;
                foreach (var row in rows)
                {
                    if (!row.StartsWith("INSERT INTO ", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Warnings.Add($"[Row {rowNumber}] Not a valid Sql INSERT statement, skipping");
                        continue;
                    }
                    var tableName = GetTableName(row);

                    // map the column name positions to the data
                    var columnMap = new Dictionary<string, int>();
                    // find the first and second group of () in the INSERT statement
                    var exp = new Regex(@"\(([^)]+)\)").Matches(row);
                    if (exp.Count == 2)
                    {
                        // map column names
                        var columnNamesText = row.Substring(exp[0].Index + 1, exp[0].Length - 2);
                        var columnNames = columnNamesText.Split(delimiter, StringSplitOptions.TrimEntries);
                        for (var i = 0; i < columnNames.Length; i++)
                        {
                            var unquotedColumnName = GetQuoted(columnNames[i]);
                            if (!string.IsNullOrEmpty(unquotedColumnName))
                                columnMap.Add(unquotedColumnName, i);
                        }

                        if (columnMap.Any())
                        {
                            // insert row
                            var columnValuesText = row.Substring(exp[1].Index + 1, exp[1].Length - 2);
                            await AddRowAsync(result, rowNumber, delimiter, columnValuesText, tableName, columnMap, partTypes, userContext);
                        }
                        else
                        {
                            result.Errors.Add($"[Row {rowNumber}] Failed to parse table column names in insert statement!");
                        }
                    }
                    else
                    {
                        result.Warnings.Add($"[Row {rowNumber}] Insert statement invalid, both column name and values are required: '{row}'");
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

        private async Task AddRowAsync(ImportResult result, int rowNumber, char delimiter, string row, string? tableName, Dictionary<string, int> columnMap, ICollection<PartType> partTypes, IUserContext? userContext)
        {
            var rowData = SplitBoundaries(row, [delimiter], true);
            if (string.IsNullOrEmpty(tableName))
                return;

            switch (tableName.ToLower())
            {
                case "projects":
                    {
                        // import project info
                        var (values, errors) = MapObject<Project>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddProjectAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "parttypes":
                    {
                        // import partTypes info
                        var (values, errors) = MapObject<PartType>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddPartTypeAsync(rowNumber, values, partTypes, result, userContext);
                    }
                    break;
                case "parts":
                    {
                        // import parts info
                        var (values, errors) = MapObject<Part>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddPartAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "partmodels":
                    {
                        // import part models
                        var (values, errors) = MapObject<PartModel>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddPartModelsAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "partparametrics":
                    {
                        // import part parametrics
                        var (values, errors) = MapObject<PartParametric>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddPartParametricsAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "customfields":
                    {
                        // import custom fields
                        var (values, errors) = MapObject<CustomField>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddCustomFieldsAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "customfieldvalues":
                    {
                        // import custom fields
                        var (values, errors) = MapObject<CustomFieldValue>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddCustomFieldValuesAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "pcbs":
                    {
                        // import project pcb's
                        var (values, errors) = MapObject<Pcb>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddPcbAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "projectpcbassignments":
                    {
                        // import project pcb assignments
                        var (values, errors) = MapObject<ProjectPcbAssignment>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddProjectPcbAssignmentAsync(rowNumber, values, result, userContext);
                    }
                    break;
                case "projectpartassignments":
                    {
                        // import project part assignments
                        var (values, errors) = MapObject<ProjectPartAssignment>(rowData, columnMap);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddProjectPartAssignmentAsync(rowNumber, values, result, userContext);
                    }
                    break;
                default:
                    result.Warnings.Add($"[Row {rowNumber}] Invalid table name in insert statement, skipping row with value(s): '{row}'");
                    break;
            }
        }

        private (Dictionary<string, object?> Values, List<string> Errors) MapObject<T>(string[] rowData, Dictionary<string, int> columnMap)
        {
            var values = new Dictionary<string, object?>();
            var errors = new List<string>();

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var @switch = new Dictionary<Type, Action> {
                    { typeof(long), () => Map<long>(property, rowData, columnMap, ref values, ref errors) },
                    { typeof(long?), () => Map<long?>(property, rowData, columnMap, ref values, ref errors) },
                    { typeof(string), () => Map<string>(property, rowData, columnMap, ref values, ref errors) },
                    { typeof(double), () => Map<double>(property, rowData, columnMap, ref values, ref errors) },
                    { typeof(decimal), () => Map<decimal>(property, rowData, columnMap, ref values, ref errors) },
                    { typeof(bool), () => Map<bool>(property, rowData, columnMap, ref values, ref errors) },
                    { typeof(int), () => Map<int>(property, rowData, columnMap, ref values, ref errors) },
                    { typeof(DateTime), () => Map<DateTime>(property, rowData, columnMap, ref values, ref errors, DateTime.UtcNow) },
                };

                var propertyType = property.PropertyType;
                if (@switch.ContainsKey(propertyType))
                    @switch[propertyType]();
            }
            return (values, errors);
        }

        protected virtual bool TryGet<T>(string?[] rowData, Dictionary<string, int>? columnMap, string name, out T? value)
        {
            value = default;
            if (columnMap == null)
                return false;
            if (!columnMap.ContainsKey(name))
                return false;
            var columnIndex = -1;
            if (columnIndex < columnMap.Count)
                columnIndex = columnMap[name];

            return TryGetValue(rowData, columnIndex, name, out value);
        }

        private void Map<T>(PropertyInfo property, string[] rowData, Dictionary<string, int>? columnMap, ref Dictionary<string, object?> values, ref List<string> errors, object? defaultValue = null)
        {
            var isSuccess = TryGet<T>(rowData, columnMap, property.Name, out var value);
            MapValue(isSuccess, value, property, ref values, ref errors, defaultValue);
        }

        private string? GetTableName(string statement)
        {
            var rowParts = statement.Split(" ");
            if (rowParts.Length < 3)
                return null;
            var unquotedVal = GetQuoted(rowParts[2]);
            // if name contains schema, only accept dbo schema prefixes
            if (unquotedVal?.Contains(".") == true)
                unquotedVal = unquotedVal.Replace("dbo.", "");
            return unquotedVal;
        }

        private string[] ReOrderInserts(string[] rows)
        {
            var orderedRows = new Dictionary<string, List<string>>();
            foreach (var row in rows)
            {
                var tableName = GetTableName(row);
                if (!string.IsNullOrEmpty(tableName))
                {
                    if (!orderedRows.ContainsKey(tableName))
                        orderedRows.Add(tableName, new List<string>());
                    orderedRows[tableName].Add(row);
                }
            }
            var result = new List<string>();
            foreach (var supportedTable in SupportedTables)
            {
                if (orderedRows.ContainsKey(supportedTable))
                    result.AddRange(orderedRows[supportedTable]);
            }
            return result.ToArray();
        }
    }
}
