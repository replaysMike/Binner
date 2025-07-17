using Binner.Global.Common;
using Binner.Model;
using NPOI.SS.UserModel;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Binner.Common.IO
{
    public class BOMImporter
    {
        // SupportedTables ordering matters when it comes to relational data!
        private readonly IStorageProvider _storageProvider;
        private readonly Regex _designatorMatch = new("^[a-zA-Z]{1,1}[0-9]{1,4}$");

        public BOMImporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        protected async void ImportSheet(Project project, IUserContext? userContext, ISheet worksheet, ImportResult result)
        {
            // parse worksheet
            var header = new Header(worksheet.GetRow(0));
            if (!header.IsValid)
            {
                result.Errors.Add($"Header doesn't have the requisite fields. Expecting Part Number, Quantity & Reference.");
                return;
            }

            for (var rowNumber = 1; rowNumber <= worksheet.LastRowNum; rowNumber++)
            {
                var rowData = worksheet.GetRow(rowNumber);
                if (rowData == null)
                    continue;

                // import BOM info
                var isPartNumberValid = TryGet<string?>(rowData, header.PartNumberIndex, out var partNumberOrValue);
                var isQuantityValid = TryGet<int>(rowData, header.QuantityIndex, out var quantity);
                var isReferenceValid = TryGet<string?>(rowData, header.ReferenceIndex, out var reference);
                var isNoteValid = TryGet<string?>(rowData, header.NoteIndex, out var note);
                var isFootprintValid = TryGet<string?>(rowData, header.FootprintIndex, out var footprint);

                if (!isPartNumberValid || !isQuantityValid || !isReferenceValid)
                    continue;

                var assignment = new ProjectPartAssignment();
                assignment.ProjectId = project.ProjectId;
                assignment.Quantity = quantity;
                assignment.Notes = note;
                assignment.ReferenceId = reference;
                assignment.SchematicReferenceId = reference;
                assignment.FootprintName = footprint;

            var part = await _storageProvider.GetPartAsync(partNumberOrValue, userContext);
                if (part != null)
                {
                    assignment.PartName = part.PartNumber;
                    assignment.PartId = part.PartId;
                }
                else
                {
                    part = new Part();
                    part.FootprintName = footprint;
                    part.DataSource = PartDataSources.DataImport;
                    part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Other;
                    if (!string.IsNullOrEmpty(reference))
                    {
                        var refMatch = _designatorMatch.Match(reference);
                        if (refMatch.Success)
                        {
                            if (string.Equals("R", refMatch.Value[0].ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Resistor;
                                part.Value = partNumberOrValue; // resistor, capacitor, inductor, etc.
                            }
                            else if (string.Equals("C", refMatch.Value[0].ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Capacitor;
                                part.Value = partNumberOrValue; // resistor, capacitor, inductor, etc.
                            }
                            else if (string.Equals("D", refMatch.Value[0].ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (string.Equals(partNumberOrValue, "LED", StringComparison.InvariantCultureIgnoreCase))
                                    part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.LED;
                                else
                                    part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Diode;
                                part.Value = partNumberOrValue; // resistor, capacitor, inductor, etc.
                            }
                            else if (string.Equals("L", refMatch.Value[0].ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Inductor;
                                part.Value = partNumberOrValue; // resistor, capacitor, inductor, etc.
                            }
                            else if (string.Equals("Q", refMatch.Value[0].ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Transistor;
                            }
                            else if (string.Equals("U", refMatch.Value[0].ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.IC;
                            }
                        }
                    }
                    part.ShortId = ShortIdGenerator.Generate();
                    part.PartNumber = partNumberOrValue;
                    part.Quantity = quantity;
                    part.UserId = userContext?.UserId;

                    part.Description = note;
                    try
                    {
                        part = await _storageProvider.AddPartAsync(part, userContext);
                    }
                    catch (Exception ex)
                    {
                        // failed to add part
                        result.Errors.Add($"[Row {rowNumber}'] Part with PartNumber '{partNumberOrValue}' could not be added. Error: {ex.Message}");
                    }

                    assignment.PartName = partNumberOrValue;
                    assignment.PartId = part.PartId;
                }

                try
                {
                    await _storageProvider.AddProjectPartAssignmentAsync(assignment, userContext);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"[Row {rowNumber}] BOM entry '{partNumberOrValue}' could not be added. Error: {ex.Message}");
                }
            }
        }

        private bool TryGet<T>(IRow rowData, int columnIndex, out T? value)
        {
            value = default;
            var type = typeof(T);
            var cellValue = columnIndex >= 0 ? rowData.GetCell(columnIndex) : null;
            if (Nullable.GetUnderlyingType(type) != null && cellValue?.ToString() == null)
                return false;
            var unquotedValue = GetQuoted(cellValue?.ToString());

            if (type == typeof(string))
            {
                if (!string.IsNullOrEmpty(unquotedValue))
                {
                    value = (T)(object)unquotedValue;
                    return true;
                }
                return false;
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

        private class Header
        {
            private bool _isValid = false;

            private int _partNumberIndex = -1;
            private int _quantityIndex = -1;
            private int _referenceIndex = -1;
            private int _noteIndex = -1;
            private int _footprintIndex = -1;

            /// <summary>
            /// the following are valid part number headers (4.7k, ATtiny4-TS etc)
            /// </summary>
            /// <remarks>
            /// KiCad: Schematic BOM = 'Value', PCB BOM = 'Designation'
            /// EasyEDA: 'Name' or 'Manufacturer Part'
            /// </remarks>
            private static readonly string[] PartNumberHeaders = { "Value", "Designation", "MPN", "Manufacturer Part", "Supplier Part", "Manufacturer_Part_Number", "PartNumber", "Mfr Part", "P/N", "Mouser P/N", "Digikey Part Number", "Part Number" };
            
            /// <summary>
            /// the following are valid quantity headers (numeric quantity)
            /// </summary>
            /// <remarks>
            /// KiCad: Schematic BOM = 'Qty', PCB BOM = 'Quantity'
            /// EasyEDA: 'Quantity'
            /// </remarks>
            private static readonly string[] QuantityHeaders = { "Qty", "Quantity", "Qty Required", "Quantity Per PCB" };
            
            /// <summary>
            /// Reference headers (R1, C1, U1, etc.)
            /// </summary>
            /// <remarks>
            /// KiCad: Schematic BOM = 'Reference', PCB BOM = 'Designator'
            /// EasyEDA: 'Designator'
            /// </remarks>
            private static readonly string[] ReferenceHeaders = { "Reference", "Designator", "SchematicReferenceId", "References", "Board ID" };
            
            /// <summary>
            /// Note/comment headers
            /// </summary>
            /// <remarks>
            /// KiCad: Schematic BOM = '', PCB BOM = ''
            /// EasyEDA: 'Comment'
            /// </remarks>
            private static readonly string[] NoteHeaders = { "Note", "Datasheet", "Supplier and ref", "Comment" };
            
            /// <summary>
            /// Footprint headers
            /// </summary>
            /// <remarks>
            /// KiCad: Schematic BOM = 'Footprint', PCB BOM = 'Footprint'
            /// EasyEDA: 'Footprint'
            /// </remarks>
            private static readonly string[] FootprintHeaders = { "Footprint" };

            public bool IsValid { get => _isValid; }

            public int PartNumberIndex => _partNumberIndex;
            public int QuantityIndex => _quantityIndex;
            public int ReferenceIndex => _referenceIndex;
            public int NoteIndex => _noteIndex;
            public int FootprintIndex => _footprintIndex;

            public Header(IRow headerRow)
            {
                _isValid = false;

                for (var i = 0; i < headerRow.LastCellNum; i++)
                {
                    var headerName = headerRow.GetCell(i).StringCellValue;
                    var name = headerName.Replace("'", "").Replace("\"", "").Replace("\n", "");

                    if (_partNumberIndex == -1)
                    {
                        if (PartNumberHeaders.FirstOrDefault(value => value.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != default)
                            _partNumberIndex = i;
                    }
                    if (_quantityIndex == -1)
                    {
                        if (QuantityHeaders.FirstOrDefault(value => value.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != default)
                            _quantityIndex = i;
                    }
                    if (_referenceIndex == -1)
                    {
                        if (ReferenceHeaders.FirstOrDefault(value => value.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != default)
                            _referenceIndex = i;
                    }
                    if (_noteIndex == -1)
                    {
                        if (NoteHeaders.FirstOrDefault(value => value.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != default)
                            _noteIndex = i;
                    }
                    if (_footprintIndex == -1)
                    {
                        if (FootprintHeaders.FirstOrDefault(value => value.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != default)
                            _footprintIndex = i;
                    }
                }

                if (_partNumberIndex != -1 && _quantityIndex != -1 && _referenceIndex != -1)
                {
                    _isValid = true;
                }
            }
        }
    }
}
