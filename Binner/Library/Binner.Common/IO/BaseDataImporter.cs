using Binner.Common.Extensions;
using Binner.Global.Common;
using Binner.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Base data importing functionality
    /// </summary>
    public class BaseDataImporter : IDataImporter
    {
        private readonly IStorageProvider _storageProvider;
        private readonly TemporaryKeyTracker _temporaryKeyTracker;

        public BaseDataImporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
            _temporaryKeyTracker = new TemporaryKeyTracker();
        }

        public virtual Task<ImportResult> ImportAsync(string filename, Stream stream, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public virtual Task<ImportResult> ImportAsync(IEnumerable<UploadFile> files, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        protected virtual bool IsNullable(PropertyInfo property)
        {
            NullabilityInfoContext nullabilityInfoContext = new NullabilityInfoContext();
            var info = nullabilityInfoContext.Create(property);
            if (info.WriteState == NullabilityState.Nullable || info.ReadState == NullabilityState.Nullable)
            {
                return true;
            }

            return false;
        }

        protected virtual async Task AddProjectAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var projectId = values.GetValue("ProjectId").As<long>();
            var name = GetQuoted(values.GetValue("Name").As<string>())?.Trim();
            if (!string.IsNullOrEmpty(name) && await _storageProvider.GetProjectAsync(name, userContext) == null)
            {
                var project = new Project
                {
                    Name = name,
                    Description = GetQuoted(values.GetValue("Description").As<string?>()),
                    Location = GetQuoted(values.GetValue("Location").As<string?>()),
                    Color = values.GetValue("Color").As<int>(),
                    DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>()
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
                    result.Errors.Add($"[Row {rowNumber}, Project with name '{name}' could not be added. Error: {ex.Message}");
                }
            }
            else
            {
                result.Warnings.Add($"[Row {rowNumber}] Project with name '{name}' already exists.");
            }
        }

        protected virtual async Task AddPartTypeAsync(int rowNumber, Dictionary<string, object?> values, ICollection<PartType> partTypes, ImportResult result, IUserContext? userContext)
        {
            var partTypeId = values.GetValue("PartTypeId").As<long>();
            var name = GetQuoted(values.GetValue("Name").As<string>())?.Trim();
            // part types need to have a unique name for the user and can not be part of global part types
            if (!string.IsNullOrEmpty(name) && !partTypes.Any(x => x.Name?.Equals(name, StringComparison.InvariantCultureIgnoreCase) == true))
            {
                var parentPartTypeId = values.GetValue("ParentPartTypeId").As<long?>();
                if (parentPartTypeId == 0) parentPartTypeId = null;
                var partType = new PartType
                {
                    ParentPartTypeId = parentPartTypeId != null ? _temporaryKeyTracker.GetMappedId("PartTypes", "PartTypeId", parentPartTypeId.Value) : null,
                    Name = name,
                    DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>()
                };
                partType = await _storageProvider.GetOrCreatePartTypeAsync(partType, userContext);
                if (partType != null)
                {
                    _temporaryKeyTracker.AddKeyMapping("PartTypes", "PartTypeId", partTypeId, partType.PartTypeId);
                    result.TotalRowsImported++;
                    result.RowsImportedByTable["PartTypes"]++;
                }
            }
            else
            {
                result.Warnings.Add($"[Row {rowNumber}] PartType with name '{name}' already exists.");
            }
        }

        protected virtual async Task AddPartAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var partId = values.GetValue("PartId").As<long>();
            var partNumber = values.GetValue("PartNumber").As<string?>() ?? string.Empty;
            if (!string.IsNullOrEmpty(partNumber) && await _storageProvider.GetPartAsync(partNumber, userContext) == null)
            {
                var part = new Part
                {
                    PartTypeId = _temporaryKeyTracker.GetMappedId("PartTypes", "PartTypeId", values.GetValue("PartTypeId").As<long>()),
                    BinNumber = values.GetValue("BinNumber").As<string?>(),
                    BinNumber2 = values.GetValue("BinNumber2").As<string?>(),
                    Cost = values.GetValue("Cost").As<double>(),
                    DatasheetUrl = values.GetValue("DatasheetUrl").As<string?>(),
                    Description = values.GetValue("Description").As<string?>(),
                    DigiKeyPartNumber = values.GetValue("DigiKeyPartNumber").As<string?>(),
                    MouserPartNumber = values.GetValue("MouserPartNumber").As<string?>(),
                    ArrowPartNumber = values.GetValue("ArrowPartNumber").As<string?>(),
                    TmePartNumber = values.GetValue("TmePartNumber").As<string?>(),
                    ImageUrl = values.GetValue("ImageUrl").As<string?>(),
                    //Keywords = !string.IsNullOrEmpty(values.GetValue("Keywords").As<string?>()) ? values.GetValue("Keywords").As<string>().Split([","," "], StringSplitOptions.RemoveEmptyEntries) : null,
                    Keywords = values.GetValue("Keywords").As<ICollection<string>>(),
                    Location = values.GetValue("Location").As<string?>(),
                    LowestCostSupplier = values.GetValue("LowestCostSupplier").As<string?>(),
                    LowestCostSupplierUrl = values.GetValue("LowestCostSupplierUrl").As<string?>(),
                    LowStockThreshold = values.GetValue("LowStockThreshold").As<int>(),
                    Manufacturer = values.GetValue("Manufacturer").As<string?>(),
                    ManufacturerPartNumber = values.GetValue("ManufacturerPartNumber").As<string?>(),
                    MountingTypeId = values.GetValue("MountingTypeId").As<int>(),
                    PackageType = values.GetValue("PackageType").As<string?>(),
                    PartNumber = partNumber,
                    ProductUrl = values.GetValue("ProductUrl").As<string?>(),
                    ProjectId = values.GetValue("ProjectId").As<long?>() != null ? _temporaryKeyTracker.GetMappedId("Projects", "ProjectId", values.GetValue("ProjectId").As<long>()) : null,
                    Quantity = values.GetValue("Quantity").As<long>(),
                    DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                    Currency = values.GetValue("Currency").As<string?>(),
                    ExtensionValue1 = values.GetValue("ExtensionValue1").As<string?>(),
                    ExtensionValue2 = values.GetValue("ExtensionValue2").As<string?>(),
                    FootprintName = values.GetValue("FootprintName").As<string?>(),
                    SymbolName = values.GetValue("Symbol").As<string?>(),
                };
                // some data validation required
                if (part.ProjectId == 0) part.ProjectId = null;
                if (part.UserId == 0) part.UserId = userContext?.UserId;
                if (part.PartTypeId == 0) part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Other;

                try
                {
                    part = await _storageProvider.AddPartAsync(part, userContext);
                    _temporaryKeyTracker.AddKeyMapping("Parts", "PartId", partId, part.PartId);
                    result.TotalRowsImported++;
                    result.RowsImportedByTable["Parts"]++;
                }
                catch (Exception ex)
                {
                    // failed to add part
                    result.Errors.Add($"[Row {rowNumber}, Part with PartNumber '{partNumber}' could not be added. Error: {ex.Message}");
                }
            }
            else
            {
                result.Warnings.Add($"[Row {rowNumber}] Part with PartNumber '{partNumber}' already exists.");
            }
        }

        protected virtual void MapValue<T>(bool isSuccess, T? value, PropertyInfo property, ref Dictionary<string, object?> values, ref List<string> errors, object? defaultValue = null)
        {
            // if model has a Key attribute, give it a default value if it wasn't provided
            if (Attribute.IsDefined(property, typeof(KeyAttribute)))
            {
                defaultValue = default(T);
            }
            if (isSuccess)
            {
                values.Add(property.Name, value);
            }
            else
            {
                if (defaultValue != null)
                    values.Add(property.Name, defaultValue);
                else if (!IsNullable(property))
                    errors.Add($"Invalid value for column '{property.Name}'");
            }
        }

        protected virtual bool TryGetValue<T>(string?[] rowData, int columnIndex, string name, out T? value)
        {
            value = default;
            var type = typeof(T);
            if (columnIndex < 0 || columnIndex >= rowData.Length)
            {
                value = default;
                return true;
            }
            if (Nullable.GetUnderlyingType(type) != null && (rowData[columnIndex] == null || rowData[columnIndex]?.Equals("null", StringComparison.InvariantCultureIgnoreCase) == true))
                return true;
            var unquotedValue = GetQuoted(rowData[columnIndex]);

            return TryGetQuotedValue(unquotedValue, out value);
        }

        protected virtual bool TryGetQuotedValue<T>(string? unquotedValue, out T? value)
        {
            value = default;
            var type = typeof(T);
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
            if (type == typeof(ICollection<string>) || type == typeof(ICollection<string?>))
            {
                if (string.IsNullOrEmpty(unquotedValue)) return true;
                value = (T)(object)unquotedValue.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get a quoted string value from the input string.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        protected virtual string? GetQuoted(string? val)
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

        /// <summary>
        /// Splits the data into rows based on the specified delimiters, handling quoted strings correctly.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rowDelimiters"></param>
        /// <param name="removeBoundary"></param>
        /// <returns></returns>
        protected virtual string[] SplitBoundaries(string data, char[] rowDelimiters, bool removeBoundary = false)
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
