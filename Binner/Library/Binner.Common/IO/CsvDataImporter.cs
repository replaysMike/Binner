using Binner.Common.Models;
using Binner.Model;
using Binner.Model.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public class CsvDataImporter : IDataImporter
    {
        // SupportedTables ordering matters when it comes to relational data!
        private readonly string[] SupportedTables = new string[] { "Projects", "PartTypes", "Parts" };
        private readonly IStorageProvider _storageProvider;
        private readonly TemporaryKeyTracker _temporaryKeyTracker = new TemporaryKeyTracker();

        public CsvDataImporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<ImportResult> ImportAsync(IEnumerable<UploadFile> files, UserContext userContext)
        {
            var result = new ImportResult();
            // is filename correct?
            foreach (var file in files)
            {
                var tableName = Path.GetFileNameWithoutExtension(file.Filename);
                if (!SupportedTables.Contains(tableName, StringComparer.InvariantCultureIgnoreCase))
                {
                    result.Errors.Add($"Filename '{tableName}' not a supported table name! Filename must be one of the following: {string.Join(",", SupportedTables)} and may optionally contain the schema prefix 'dbo'.");
                    result.Success = false;
                    return result;
                }
            }
            // order files by declaration order in SupportedTables
            var filesOrdered = new List<UploadFile>();
            foreach (var table in SupportedTables)
            {
                var file = files.FirstOrDefault(x => x.Filename.StartsWith($"{table}.", StringComparison.InvariantCultureIgnoreCase));
                if (file != null)
                    filesOrdered.Add(file);
            }
            foreach (var file in filesOrdered)
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

        public async Task<ImportResult> ImportAsync(string filename, Stream stream, UserContext userContext)
        {
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
                // remove line breaks
                data = data.Replace("\r", "");

                var tableName = Path.GetFileNameWithoutExtension(filename);
                if (!SupportedTables.Contains(tableName, StringComparer.InvariantCultureIgnoreCase))
                {
                    result.Errors.Add($"Filename '{tableName}' not a supported table name! Filename must be one of the following: {string.Join(",", SupportedTables)} and may optionally contain the schema prefix 'dbo'.");
                    result.Success = false;
                    return result;
                }

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
                if (headerRow.StartsWith("#"))
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
                    switch (tableName.ToLower())
                    {
                        case "projects":
                            {
                                // import project info
                                var isProjectIdValid = TryGet<long>(rowData, header, "ProjectId", out var projectId);
                                var isColorValid = TryGet<int>(rowData, header, "Color", out var color);
                                var isDateCreatedValid = TryGet<DateTime>(rowData, header, "DateCreatedUtc", out var dateCreatedUtc);
                                var isDateModifiedValid = TryGet<DateTime>(rowData, header, "DateModifiedUtc", out var dateModifiedUtc);
                                if (!isColorValid || !isDateCreatedValid || !isDateModifiedValid)
                                    continue;
                                var name = GetQuoted(rowData[header.GetHeaderIndex("Name")])?.Trim();

                                if (!string.IsNullOrEmpty(name) && await _storageProvider.GetProjectAsync(name, userContext) == null)
                                {
                                    var project = new Project
                                    {
                                        Name = name,
                                        Description = GetQuoted(rowData[header.GetHeaderIndex("Description")]),
                                        Location = GetQuoted(rowData[header.GetHeaderIndex("Location")]),
                                        Color = color,
                                        DateCreatedUtc = dateCreatedUtc,
                                        //DateModifiedUtc = dateModifiedUtc,
                                        UserId = userContext.UserId
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
                                var isPartTypeIdValid = TryGet<long>(rowData, header, "PartTypeId", out var partTypeId);
                                var isParentPartTypeIdValid = TryGet<long?>(rowData, header, "ParentPartTypeId", out var parentPartTypeId);
                                var isDateCreatedValid = TryGet<DateTime>(rowData, header, "DateCreatedUtc", out var dateCreatedUtc);
                                if (!isParentPartTypeIdValid || !isDateCreatedValid)
                                    continue;

                                var name = GetQuoted(rowData[header.GetHeaderIndex("Name")])?.Trim();
                                // part types need to have a unique name for the user and can not be part of global part types
                                if (!string.IsNullOrEmpty(name) && !partTypes.Any(x => x.Name?.Equals(name, StringComparison.InvariantCultureIgnoreCase) == true))
                                {
                                    var partType = new PartType
                                    {
                                        ParentPartTypeId = parentPartTypeId != null ? _temporaryKeyTracker.GetMappedId("PartTypes", "PartTypeId", parentPartTypeId.Value) : null,
                                        Name = name,
                                        DateCreatedUtc = dateCreatedUtc,
                                        UserId = userContext.UserId
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
                                var isPartIdValid = TryGet<long>(rowData, header, "PartId", out var partId);
                                var isPartTypeIdValid = TryGet<long>(rowData, header, "PartTypeId", out var partTypeId);
                                var isBinNumberValid = TryGet<string?>(rowData, header, "BinNumber", out var binNumber);
                                var isBinNumber2Valid = TryGet<string?>(rowData, header, "BinNumber2", out var binNumber2);
                                var isCostValid = TryGet<double>(rowData, header, "Cost", out var cost);
                                var isDatasheetUrlValid = TryGet<string?>(rowData, header, "DatasheetUrl", out var datasheetUrl);
                                var isDescriptionValid = TryGet<string?>(rowData, header, "Description", out var description);
                                var isDigiKeyPartNumberValid = TryGet<string?>(rowData, header, "DigiKeyPartNumber", out var digiKeyPartNumber);
                                var isImageUrlValid = TryGet<string?>(rowData, header, "ImageUrl", out var imageUrl);
                                var isKeywordsValid = TryGet<string?>(rowData, header, "Keywords", out var keywords);
                                var isLocationValid = TryGet<string?>(rowData, header, "Location", out var location);
                                var isLowestCostSupplierValid = TryGet<string?>(rowData, header, "LowestCostSupplier", out var lowestCostSupplier);
                                var isLowestCostSupplierUrlValid = TryGet<string?>(rowData, header, "LowestCostSupplierUrl", out var lowestCostSupplierUrl);
                                var isLowStockThresholdValid = TryGet<int>(rowData, header, "LowStockThreshold", out var lowStockThreshold);
                                var isManufacturerValid = TryGet<string?>(rowData, header, "Manufacturer", out var manufacturer);
                                var isManufacturerPartNumberValid = TryGet<string?>(rowData, header, "ManufacturerPartNumber", out var manufacturerPartNumber);
                                var isMountingTypeIdValid = TryGet<int>(rowData, header, "MountingTypeId", out var mountingTypeId);
                                var isMouserPartNumberValid = TryGet<string?>(rowData, header, "MouserPartNumber", out var mouserPartNumber);
                                var isPackageTypeValid = TryGet<string?>(rowData, header, "PackageType", out var packageType);
                                var isPartNumberValid = TryGet<string?>(rowData, header, "PartNumber", out var partNumber);
                                var isProductUrlValid = TryGet<string?>(rowData, header, "ProductUrl", out var productUrl);
                                var isProjectIdValid = TryGet<long?>(rowData, header, "ProjectId", out var projectId);
                                var isQuantityValid = TryGet<long>(rowData, header, "Quantity", out var quantity);
                                var isSwarmPartNumberManufacturerIdValid = TryGet<int?>(rowData, header, "SwarmPartNumberManufacturerId", out var swarmPartNumberManufacturerId);
                                var isDateCreatedValid = TryGet<DateTime>(rowData, header, "DateCreatedUtc", out var dateCreatedUtc);

                                if (!isPartTypeIdValid || !isBinNumberValid || !isBinNumber2Valid || !isCostValid || !isDatasheetUrlValid
                                    || !isDescriptionValid || !isDigiKeyPartNumberValid || !isImageUrlValid || !isKeywordsValid || !isLocationValid || !isLowestCostSupplierValid
                                    || !isLowestCostSupplierUrlValid || !isLowStockThresholdValid || !isManufacturerValid || !isManufacturerPartNumberValid || !isMountingTypeIdValid || !isMouserPartNumberValid
                                    || !isPackageTypeValid || !isPartNumberValid || !isProductUrlValid || !isProjectIdValid || !isQuantityValid || !isSwarmPartNumberManufacturerIdValid)
                                    continue;

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
                                        UserId = userContext.UserId
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

        private bool TryGet<T>(string[] rowData, Header header, string name, out T? value)
        {
            value = default;
            var type = typeof(T);
            var columnIndex = header.GetHeaderIndex(name);
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

        private string? GetQuoted(string val)
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
                var header = Headers.First(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
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
