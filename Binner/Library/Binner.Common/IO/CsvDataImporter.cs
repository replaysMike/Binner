using Binner.Global.Common;
using Binner.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Imports data from CSV files
    /// </summary>
    public class CsvDataImporter : BaseDataImporter
    {
        // SupportedTables ordering matters when it comes to relational data!
        private readonly string[] SupportedTables = ["Projects", "PartTypes", "Parts"];
        private readonly IStorageProvider _storageProvider;

        public CsvDataImporter(IStorageProvider storageProvider) : base(storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public override async Task<ImportResult> ImportAsync(IEnumerable<UploadFile> files, IUserContext? userContext)
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

        public override async Task<ImportResult> ImportAsync(string filename, Stream stream, IUserContext? userContext)
        {
            const char delimiter = '\n';
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

                var rows = SplitBoundaries(data, [delimiter]);
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
                    await AddRowAsync(result, rowNumber, row, tableName, header, partTypes, userContext);
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

        private async Task AddRowAsync(ImportResult result, int rowNumber, string row, string? tableName, Header header, ICollection<PartType> partTypes, IUserContext? userContext)
        {
            var rowData = SplitBoundaries(row, [','], true);
            if (string.IsNullOrEmpty(tableName))
                return;
            if (rowData.Length != header.Headers.Count)
            {
                result.Warnings.Add($"[Row {rowNumber}] Row does not contain the same number of columns as the header, skipping...");
                return;
            }

            switch (tableName.ToLower())
            {
                case "projects":
                    {
                        // import project info
                        var (values, errors) = MapObject<Project>(rowData, header);
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
                        var (values, errors) = MapObject<PartType>(rowData, header);
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
                        var (values, errors) = MapObject<Part>(rowData, header);
                        foreach (var err in errors)
                        {
                            result.Errors.Add($"[Row {rowNumber}] {err}, skipping: '{row}'");
                            return;
                        }

                        await AddPartAsync(rowNumber, values, result, userContext);
                    }
                    break;
            }
        }

        private (Dictionary<string, object?> Values, List<string> Errors) MapObject<T>(string[] rowData, Header header)
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
                    { typeof(ICollection<string>), () => Map<ICollection<string>>(property, rowData, header, ref values, ref errors) },
                };

                var propertyType = property.PropertyType;
                if (@switch.ContainsKey(propertyType))
                    @switch[propertyType]();
            }
            return (values, errors);
        }

        protected virtual bool TryGet<T>(string?[] rowData, Header header, string name, out T? value)
        {
            value = default;
            var columnIndex = header.GetHeaderIndex(name);

            return TryGetValue(rowData, columnIndex, name, out value);
        }

        private void Map<T>(PropertyInfo property, string[] rowData, Header header, ref Dictionary<string, object?> values, ref List<string> errors, object? defaultValue = null)
        {
            var isSuccess = TryGet<T>(rowData, header, property.Name, out var value);
            MapValue(isSuccess, value, property, ref values, ref errors, defaultValue);
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
