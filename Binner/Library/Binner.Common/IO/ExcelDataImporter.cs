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
using TypeSupport.Extensions;

namespace Binner.Common.IO
{
    /// <summary>
    /// Imports data from Excel Open XML Format 2007+ (XLSX)
    /// </summary>
    public class ExcelDataImporter : IDataImporter
    {
        // SupportedTables ordering matters when it comes to relational data!
        private readonly string[] SupportedTables = new string[] { "Projects", "PartTypes", "Parts", "BOM" };
        private readonly IStorageProvider _storageProvider;
        private readonly TemporaryKeyTracker _temporaryKeyTracker = new TemporaryKeyTracker();

        public ExcelDataImporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<ImportResult> ImportAsync(IEnumerable<UploadFile> files, IUserContext? userContext)
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

        private string? GetCellValue(Header header, IRow? rowData, string name)
        {
            var index = header.GetHeaderIndex(name);
            if (index >= 0)
                return rowData?.GetCell(index)?.ToString();
            return null;
        }

        public async Task<ImportResult> ImportAsync(string filename, Stream stream, IUserContext? userContext)
        {
            var result = new ImportResult();
            foreach (var table in SupportedTables)
                result.RowsImportedByTable.Add(table, 0);

            long bomProjectId = 0;
            if (filename.Contains("_bom"))
            {
                string projectName = filename.Remove(filename.LastIndexOf("_bom"));
                Project project = await _storageProvider.GetProjectAsync(projectName, userContext);
                if (project == null)
                {
                    project = new Project();
                    project.Name = projectName;
                    try
                    {
                        project = await _storageProvider.AddProjectAsync(project, userContext);
                        bomProjectId = project.ProjectId;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"[Sheet '{projectName}'] Project with name '{projectName}' could not be added. Error: {ex.Message}");
                    }

                }
                else
                {
                    bomProjectId = project.ProjectId;
                }

            }
            // get the global part types, and the user's custom part types
            var partTypes = (await _storageProvider.GetPartTypesAsync(userContext)).ToList();
            try
            {
                stream.Position = 0;
                var workbook = WorkbookFactory.Create(stream);
                foreach (var table in SupportedTables)
                {
                    var worksheet = workbook.GetSheet(table);
                    if (worksheet != null)
                    {
                        // parse worksheet
                        var header = new Header(worksheet.GetRow(0));
                        for (var rowNumber = 1; rowNumber <= worksheet.LastRowNum; rowNumber++)
                        {
                            var rowData = worksheet.GetRow(rowNumber);
                            if (rowData == null)
                                continue;
                            switch (table.ToLower())
                            {
                                case "bom":
                                    {
                                        // import BOM info
                                        var isPartNumberValid = TryGet<string?>(rowData, header, "MPN", out var partNumber);
                                        var isQuantityValid = TryGet<int>(rowData, header, "Quantity per PCB", out var quantity);
                                        var isReferenceValid = TryGet<string?>(rowData, header, "References", out var reference);
                                        var isNoteValid = TryGet<string?>(rowData, header, "Value", out var note);

                                        if (!isPartNumberValid || !isQuantityValid || !isReferenceValid)
                                            continue;

                                        ProjectPartAssignment assignment = new ProjectPartAssignment();
                                        assignment.ProjectId = bomProjectId;
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
                                                result.Errors.Add($"[Row {rowNumber}, Sheet '{table}'] Part with PartNumber '{partNumber}' could not be added. Error: {ex.Message}");
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
                                            result.Errors.Add($"[Row {rowNumber}, Sheet '{table}'] BOM entry '{partNumber}' could not be added. Error: {ex.Message}");
                                        }

                                    }
                                    break;
                                case "projects":
                                    {
                                        // import project info
                                        var isProjectIdValid = TryGet<long>(rowData, header, "ProjectId", out var projectId);
                                        var isColorValid = TryGet<int>(rowData, header, "Color", out var color);
                                        var isDateCreatedValid = TryGet<DateTime>(rowData, header, "DateCreatedUtc", out var dateCreatedUtc);
                                        var isDateModifiedValid = TryGet<DateTime>(rowData, header, "DateModifiedUtc", out var dateModifiedUtc);
                                        if (!isColorValid || !isDateCreatedValid || !isDateModifiedValid)
                                            continue;
                                        var cellValue = GetCellValue(header, rowData, "Name");
                                        if (string.IsNullOrEmpty(cellValue))
                                            continue;
                                        var name = GetQuoted(cellValue)?.Trim();

                                        if (!string.IsNullOrEmpty(name) && await _storageProvider.GetProjectAsync(name, userContext) == null)
                                        {
                                            var project = new Project
                                            {
                                                Name = name,
                                                Description = GetQuoted(GetCellValue(header, rowData, "Description")),
                                                Location = GetQuoted(GetCellValue(header, rowData, "Location")),
                                                Color = color,
                                                DateCreatedUtc = dateCreatedUtc,
                                                //DateModifiedUtc = dateModifiedUtc
                                            };
                                            try
                                            {
                                                project = await _storageProvider.AddProjectAsync(project, userContext);
                                                _temporaryKeyTracker.AddKeyMapping("Projects", "ProjectId", projectId, project.ProjectId);
                                                result.TotalRowsImported++;
                                                result.RowsImportedByTable["Projects"]++;
                                            }
                                            catch (Exception ex)
                                            {
                                                result.Errors.Add($"[Row {rowNumber}, Sheet '{table}'] Project with name '{name}' could not be added. Error: {ex.Message}");
                                            }
                                        }
                                        else
                                        {
                                            result.Warnings.Add($"[Row {rowNumber}, Sheet '{table}'] Project with name '{name}' already exists.");
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

                                        var cellValue = GetCellValue(header, rowData, "Name");
                                        if (string.IsNullOrEmpty(cellValue))
                                            continue;

                                        var name = GetQuoted(cellValue)?.Trim();
                                        // part types need to have a unique name for the user and can not be part of global part types
                                        if (!string.IsNullOrEmpty(name) && !partTypes.Any(x => x.Name != null && x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                                        {
                                            if (parentPartTypeId == 0)
                                                parentPartTypeId = null;
                                            var partType = new PartType
                                            {
                                                ParentPartTypeId = parentPartTypeId != null ? _temporaryKeyTracker.GetMappedId("PartTypes", "PartTypeId", parentPartTypeId.Value) : null,
                                                Name = name,
                                                DateCreatedUtc = dateCreatedUtc
                                            };
                                            partType = await _storageProvider.GetOrCreatePartTypeAsync(partType, userContext);
                                            if (partType != null)
                                            {
                                                _temporaryKeyTracker.AddKeyMapping("PartTypes", "PartTypeId",
                                                    partTypeId, partType.PartTypeId);
                                                result.TotalRowsImported++;
                                                result.RowsImportedByTable["PartTypes"]++;
                                            }
                                        }
                                        else
                                        {
                                            result.Warnings.Add($"[Row {rowNumber}, Sheet '{table}'] PartType with name '{name}' already exists.");
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
                                                Cost = cost,
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
                                                DateCreatedUtc = dateCreatedUtc
                                            };
                                            // some data validation required
                                            if (part.ProjectId == 0) part.ProjectId = null;
                                            if (part.UserId == 0) part.UserId = userContext?.UserId;
                                            if (part.PartTypeId == 0) part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Other;
                                            try
                                            {
                                                part = await _storageProvider.AddPartAsync(part, userContext);
                                            }
                                            catch (Exception ex) 
                                            {
                                                // failed to add part
                                                result.Errors.Add($"[Row {rowNumber}, Sheet '{table}'] Part with PartNumber '{partNumber}' could not be added. Error: {ex.Message}");
                                            }

                                            _temporaryKeyTracker.AddKeyMapping("Parts", "PartId", partId, part.PartId);
                                            result.TotalRowsImported++;
                                            result.RowsImportedByTable["Parts"]++;
                                        }
                                        else
                                        {
                                            result.Warnings.Add($"[Row {rowNumber}, Sheet '{table}'] Part with PartNumber '{partNumber}' already exists.");
                                        }
                                    }
                                    break;
                            }
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
