using Binner.Global.Common;
using Binner.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using TypeSupport;
using TypeSupport.Extensions;

namespace Binner.Common.IO
{
    /// <summary>
    /// Builds a dataset from an IBinnerDb
    /// </summary>
    public class DataSetBuilder : IBuilder<DataSet>
    {
        /// <summary>
        /// List of types considered numeric
        /// </summary>
        private readonly List<Type> _numericTypes = new List<Type> { typeof(byte), typeof(sbyte), typeof(ushort), typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(float), typeof(double), typeof(decimal) };
        
        /// <summary>
        /// Supported list of types we will export from models.
        /// </summary>
        private readonly List<Type> _exportableColumnTypes = new List<Type> { typeof(byte), typeof(sbyte), typeof(ushort), typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(string), typeof(Enum), typeof(Guid), typeof(bool), typeof(DateTime), typeof(ICollection<string>) };
        
        public class TableContainer
        {
            public DataTable Table { get; }
            public IEnumerable Data { get; }
            public ExtendedType? ExtendedType { get; set; }

            public TableContainer(DataTable table, IEnumerable data)
            {
                Table = table;
                Data = data;
            }

            public static TableContainer Create(string tableName, IEnumerable data)
            {
                return new TableContainer(new DataTable(tableName), data);
            }
        }

        /// <summary>
        /// Build a dataset from an IBinnerDb object
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public DataSet Build(IBinnerDb db)
        {
            var locale = Thread.CurrentThread.CurrentCulture;
            var dataSet = new DataSet("Datset");
            dataSet.Locale = locale;

            // define the tables we want to export from the IBinnerDb
            var tables = new Dictionary<Type, TableContainer>()
            {
                {typeof(Part), TableContainer.Create("Parts", db.Parts) },
                {typeof(PartType), TableContainer.Create("PartTypes", db.PartTypes) },
                {typeof(Project), TableContainer.Create("Projects", db.Projects) },
                {typeof(PartParametric), TableContainer.Create("PartParametrics", db.PartParametrics) },
                {typeof(PartModel), TableContainer.Create("PartModels", db.PartModels) },
                {typeof(CustomField), TableContainer.Create("CustomFields", db.CustomFields) },
                {typeof(CustomFieldValue), TableContainer.Create("CustomFieldValues", db.CustomFieldValues) },
                {typeof(Pcb), TableContainer.Create("Pcbs", db.Pcbs) },
                {typeof(ProjectPcbAssignment), TableContainer.Create("ProjectPcbAssignments", db.ProjectPcbAssignments) },
                {typeof(ProjectPartAssignment), TableContainer.Create("ProjectPartAssignments", db.ProjectPartAssignments) },
            };

            // add the tables to the dataset
            dataSet.Tables.AddRange(tables.Values.Select(x => x.Table).ToArray());

            // build the table schema
            foreach(var table in tables)
            {
                table.Value.Table.Locale = locale;
                table.Value.ExtendedType = table.Key.GetExtendedType();
                foreach (var prop in table.Value.ExtendedType.Properties)
                {
                    // only export columns if we support the type
                    if (_exportableColumnTypes.Contains(prop.Type)
                        // support nullable types
                        || (prop.Type.IsNullable && _exportableColumnTypes.Contains(prop.Type.NullableBaseType))
                        // support enums if allowed
                        || (prop.Type.IsEnum && _exportableColumnTypes.Contains(typeof(Enum)))
                        )
                    {
                        table.Value.Table.Columns.Add(prop.Name, TranslateType(prop.Type));
                    }
                    else
                    {
                        // type is not configured as an exportable
                        System.Diagnostics.Debug.WriteLine($"Skipping type '{prop.Type.FullName}' as a table exportable value.");
                    }
                }
            }

            // populate the DataTable's with data from the IBinnerDb
            foreach(var table in tables)
            {
                var dataTable = table.Value.Table;
                var extendedType = table.Value.ExtendedType ?? throw new InvalidOperationException($"ExtendedType was not created for type '{table.Key.Name}'!");
                foreach (var entry in table.Value.Data)
                {
                    var row = dataTable.NewRow();
                    foreach(var prop in extendedType.Properties)
                    {
                        var dataType = dataTable.Columns[prop.Name]?.DataType;
                        if (dataType != null)
                        {
                            try
                            {
                                var val = TranslateValue(entry.GetPropertyValue(prop), dataType, prop.Type);
                                row[prop.Name] = val;
                            }
                            catch (Exception ex)
                            {
                                // type not supported
                            }
                        }
                    }
                    dataTable.Rows.Add(row);
                }
            }

            return dataSet;
        }

        private object? DefaultValue(Type type)
        {
            if (type == typeof(string))
                return "";
            if (type == typeof(DateTime))
                return DateTime.MinValue;
            if (type == typeof(TimeSpan))
                return TimeSpan.MinValue.ToString();
            return Activator.CreateInstance(Nullable.GetUnderlyingType(type) ?? type);
        }

        private object? TranslateValue(object val, Type rowType, Type originalType)
        {
            var newVal = val;
            var originalExtendedType = originalType.GetExtendedType();
            if (_numericTypes.Contains(originalExtendedType.UnderlyingType))
                newVal = Convert.ToDouble(val);
            if (originalExtendedType.IsCollection)
            {
                // special case for collapsing string arrays into a single string
                if (rowType == typeof(string))
                    newVal = string.Join(",", (ICollection<string>)val);
            }
            if (originalExtendedType.UnderlyingType == typeof(TimeSpan))
                newVal = ((TimeSpan)val).ToString();
            return newVal ?? DefaultValue(rowType);
        }

        private Type TranslateType(Type type)
        {
            var translatedType = type;
            var extendedType = type.GetExtendedType();
            // special case for collapsing string arrays into a single string
            if (extendedType.IsCollection && extendedType.ElementType == typeof(string))
                translatedType = extendedType.ElementType;
            if (_numericTypes.Contains(extendedType.UnderlyingType))
                translatedType = typeof(double);
            if (extendedType.UnderlyingType == typeof(TimeSpan))
                translatedType = typeof(string);
            return Nullable.GetUnderlyingType(translatedType) ?? translatedType;
        }
    }
}
