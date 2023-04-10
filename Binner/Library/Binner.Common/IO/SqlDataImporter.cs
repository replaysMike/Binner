using Binner.Common.Models;
using Binner.Model.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public class SqlDataImporter : IDataImporter
    {
        // SupportedTables ordering matters when it comes to relational data!
        private readonly string[] SupportedTables = new string[] { "Projects", "PartTypes", "Parts" };
        private readonly IStorageProvider _storageProvider;
        private readonly TemporaryKeyTracker _temporaryKeyTracker = new TemporaryKeyTracker();

        public SqlDataImporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<ImportResult> ImportAsync(IEnumerable<UploadFile> files, UserContext? userContext)
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

        public async Task<ImportResult> ImportAsync(string filename, Stream stream, UserContext? userContext)
        {
            const char delimiter = ',';
            var result = new ImportResult();
            foreach (var table in SupportedTables)
                result.RowsImportedByTable.Add(table, 0);
            // get the global part types, and the user's custom part types
            var partTypes = (await _storageProvider.GetPartTypesAsync(userContext)).ToList();
            try
            {
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var data = await reader.ReadToEndAsync();
                // remove line breaks if present
                data = data.Replace("\r", "");

                var rows = SplitBoundaries(data, new char[] { '\n', ';' });
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

        private async Task AddRowAsync(ImportResult result, int rowNumber, char delimiter, string row, string? tableName, Dictionary<string, int> columnMap, ICollection<PartType> partTypes, UserContext? userContext)
        {
            var rowData = SplitBoundaries(row, new char[] { delimiter }, true);
            if (string.IsNullOrEmpty(tableName))
                return;

            switch (tableName.ToLower())
            {
                case "projects":
                    {
                        // import project info
                        var isProjectIdValid = TryGet<int>(rowData, columnMap, "ProjectId", out var projectId);
                        var isColorValid = TryGet<int>(rowData, columnMap, "Color", out var color);
                        var isDateCreatedValid = TryGet<DateTime>(rowData, columnMap, "DateCreatedUtc", out var dateCreatedUtc);
                        var isDateModifiedValid = TryGet<DateTime>(rowData, columnMap, "DateModifiedUtc", out var dateModifiedUtc);
                        if (!isColorValid || !isDateCreatedValid || !isDateModifiedValid)
                        {
                            result.Warnings.Add($"[Row {rowNumber}] Row contains invalid data, skipping: '{row}'");
                            return;
                        }
                        var name = GetQuoted(rowData[columnMap["Name"]])?.Trim();

                        if (!string.IsNullOrEmpty(name) && await _storageProvider.GetProjectAsync(name, userContext) == null)
                        {
                            var project = new Project
                            {
                                Name = name,
                                Description = GetQuoted(rowData[columnMap["Description"]]),
                                Location = GetQuoted(rowData[columnMap["Location"]]),
                                Color = color,
                                DateCreatedUtc = dateCreatedUtc,
                                //DateModifiedUtc = dateModifiedUtc,
                                UserId = userContext?.UserId
                            };
                            project = await _storageProvider.AddProjectAsync(project, userContext);
                            _temporaryKeyTracker.AddKeyMapping("Projects", "ProjectId", projectId, project.ProjectId);
                            result.TotalRowsImported++;
                            result.RowsImportedByTable["Projects"]++;
                        }
                        else
                        {
                            result.Warnings.Add($"[Row {rowNumber}] Project with name '{name}' already exists.");
                        }
                    }
                    break;
                case "parttypes":
                    {
                        // import partTypes info
                        var isPartTypeIdValid = TryGet<long>(rowData, columnMap, "PartTypeId", out var partTypeId);
                        var isParentPartTypeIdValid = TryGet<long?>(rowData, columnMap, "ParentPartTypeId", out var parentPartTypeId);
                        var isDateCreatedValid = TryGet<DateTime>(rowData, columnMap, "DateCreatedUtc", out var dateCreatedUtc);
                        if (!isParentPartTypeIdValid || !isDateCreatedValid)
                        {
                            result.Warnings.Add($"[Row {rowNumber}] Row contains invalid data, skipping: '{row}'");
                            return;
                        }

                        var name = GetQuoted(rowData[columnMap["Name"]])?.Trim();
                        // part types need to have a unique name for the user and can not be part of global part types
                        if (!string.IsNullOrEmpty(name) && !partTypes.Any(x => x?.Name?.Equals(name, StringComparison.InvariantCultureIgnoreCase) == true))
                        {
                            var partType = new PartType
                            {
                                ParentPartTypeId = parentPartTypeId != null ? _temporaryKeyTracker.GetMappedId("PartTypes", "PartTypeId", parentPartTypeId.Value) : null,
                                Name = name,
                                DateCreatedUtc = dateCreatedUtc,
                                UserId = userContext?.UserId
                            };
                            partType = await _storageProvider.GetOrCreatePartTypeAsync(partType, userContext);
                            if (partType != null)
                            {
                                _temporaryKeyTracker.AddKeyMapping("PartTypes", "PartTypeId", partTypeId,
                                    partType.PartTypeId);
                                result.TotalRowsImported++;
                                result.RowsImportedByTable["PartTypes"]++;
                            }
                        }
                        else
                        {
                            result.Warnings.Add($"[Row {rowNumber}] PartType with name '{name}' already exists.");
                        }
                    }
                    break;
                case "parts":
                    {
                        // import parts info
                        var isPartIdValid = TryGet<long>(rowData, columnMap, "PartId", out var partId);
                        var isPartTypeIdValid = TryGet<long>(rowData, columnMap, "PartTypeId", out var partTypeId);
                        var isBinNumberValid = TryGet<string?>(rowData, columnMap, "BinNumber", out var binNumber);
                        var isBinNumber2Valid = TryGet<string?>(rowData, columnMap, "BinNumber2", out var binNumber2);
                        var isCostValid = TryGet<double>(rowData, columnMap, "Cost", out var cost);
                        var isDatasheetUrlValid = TryGet<string?>(rowData, columnMap, "DatasheetUrl", out var datasheetUrl);
                        var isDescriptionValid = TryGet<string?>(rowData, columnMap, "Description", out var description);
                        var isDigiKeyPartNumberValid = TryGet<string?>(rowData, columnMap, "DigiKeyPartNumber", out var digiKeyPartNumber);
                        var isImageUrlValid = TryGet<string?>(rowData, columnMap, "ImageUrl", out var imageUrl);
                        var isKeywordsValid = TryGet<string?>(rowData, columnMap, "Keywords", out var keywords);
                        var isLocationValid = TryGet<string?>(rowData, columnMap, "Location", out var location);
                        var isLowestCostSupplierValid = TryGet<string?>(rowData, columnMap, "LowestCostSupplier", out var lowestCostSupplier);
                        var isLowestCostSupplierUrlValid = TryGet<string?>(rowData, columnMap, "LowestCostSupplierUrl", out var lowestCostSupplierUrl);
                        var isLowStockThresholdValid = TryGet<int>(rowData, columnMap, "LowStockThreshold", out var lowStockThreshold);
                        var isManufacturerValid = TryGet<string?>(rowData, columnMap, "Manufacturer", out var manufacturer);
                        var isManufacturerPartNumberValid = TryGet<string?>(rowData, columnMap, "ManufacturerPartNumber", out var manufacturerPartNumber);
                        var isMountingTypeIdValid = TryGet<int>(rowData, columnMap, "MountingTypeId", out var mountingTypeId);
                        var isMouserPartNumberValid = TryGet<string?>(rowData, columnMap, "MouserPartNumber", out var mouserPartNumber);
                        var isPackageTypeValid = TryGet<string?>(rowData, columnMap, "PackageType", out var packageType);
                        var isPartNumberValid = TryGet<string?>(rowData, columnMap, "PartNumber", out var partNumber);
                        var isProductUrlValid = TryGet<string?>(rowData, columnMap, "ProductUrl", out var productUrl);
                        var isProjectIdValid = TryGet<long?>(rowData, columnMap, "ProjectId", out var projectId);
                        var isQuantityValid = TryGet<long>(rowData, columnMap, "Quantity", out var quantity);
                        var isSwarmPartNumberManufacturerIdValid = TryGet<int?>(rowData, columnMap, "SwarmPartNumberManufacturerId", out var swarmPartNumberManufacturerId);
                        var isDateCreatedValid = TryGet<DateTime>(rowData, columnMap, "DateCreatedUtc", out var dateCreatedUtc);

                        if (!isPartTypeIdValid || !isBinNumberValid || !isBinNumber2Valid || !isCostValid || !isDatasheetUrlValid
                            || !isDescriptionValid || !isDigiKeyPartNumberValid || !isImageUrlValid || !isKeywordsValid || !isLocationValid || !isLowestCostSupplierValid
                            || !isLowestCostSupplierUrlValid || !isLowStockThresholdValid || !isManufacturerValid || !isManufacturerPartNumberValid || !isMountingTypeIdValid || !isMouserPartNumberValid
                            || !isPackageTypeValid || !isPartNumberValid || !isProductUrlValid || !isProjectIdValid || !isQuantityValid || !isSwarmPartNumberManufacturerIdValid
                            || !isDateCreatedValid)
                        {
                            result.Warnings.Add($"[Row {rowNumber}] Row contains invalid data, skipping: '{row}'");
                            return;
                        }

                        if (!string.IsNullOrEmpty(partNumber) && await _storageProvider.GetPartAsync(partNumber, userContext) == null)
                        {
                            var part = new Part
                            {
                                PartTypeId = _temporaryKeyTracker.GetMappedId("PartTypes", "PartTypeId", partTypeId),
                                BinNumber = binNumber,
                                BinNumber2 = binNumber2,
                                Cost = (decimal)cost,
                                DatasheetUrl = datasheetUrl,
                                Description = description,
                                DigiKeyPartNumber = digiKeyPartNumber,
                                ImageUrl = imageUrl,
                                Keywords = !string.IsNullOrEmpty(keywords) ? keywords.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) : null,
                                Location = location,
                                LowestCostSupplier = lowestCostSupplier,
                                LowestCostSupplierUrl = lowestCostSupplierUrl,
                                LowStockThreshold = lowStockThreshold,
                                Manufacturer = manufacturer,
                                ManufacturerPartNumber = manufacturerPartNumber,
                                MountingTypeId = mountingTypeId,
                                MouserPartNumber = mouserPartNumber,
                                PackageType = packageType,
                                PartNumber = partNumber,
                                ProductUrl = productUrl,
                                ProjectId = projectId != null ? _temporaryKeyTracker.GetMappedId("Projects", "ProjectId", projectId.Value) : null,
                                Quantity = quantity,
                                //SwarmPartNumberManufacturerId = swarmPartNumberManufacturerId,
                                DateCreatedUtc = dateCreatedUtc,
                                UserId = userContext?.UserId
                            };
                            part = await _storageProvider.AddPartAsync(part, userContext);
                            _temporaryKeyTracker.AddKeyMapping("Parts", "PartId", partId, part.PartId);
                            result.TotalRowsImported++;
                            result.RowsImportedByTable["Parts"]++;
                        }
                        else
                        {
                            result.Warnings.Add($"[Row {rowNumber}] Part with PartNumber '{partNumber}' already exists.");
                        }
                    }
                    break;
                default:
                    result.Warnings.Add($"[Row {rowNumber}] Invalid table name in insert statement, skipping: '{row}'");
                    break;
            }
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

        private bool TryGet<T>(string?[] rowData, Dictionary<string, int>? columnMap, string name, out T? value)
        {
            value = default;
            if (columnMap == null)
                return false;
            var type = typeof(T);
            if (!columnMap.ContainsKey(name))
                return false;
            var columnIndex = columnMap[name];
            if (Nullable.GetUnderlyingType(type) != null && (rowData[columnIndex] == null || rowData[columnIndex]?.Equals("null", StringComparison.InvariantCultureIgnoreCase) == true))
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
                var isIntValid = int.TryParse(unquotedValue, out var boolValue);
                value = (T)(object)boolValue;
                return isIntValid;
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                var isDateValid = false;
                if (!string.IsNullOrEmpty(unquotedValue))
                {
                    isDateValid = DateTime.TryParse(unquotedValue, out var dateValue);
                    value = (T)(object)dateValue;
                }
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
    }
}
