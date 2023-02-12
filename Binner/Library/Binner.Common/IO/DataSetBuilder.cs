using Binner.Model.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using TypeSupport.Extensions;

namespace Binner.Common.IO
{
    /// <summary>
    /// Builds a dataset from an IBinnerDb
    /// </summary>
    public class DataSetBuilder : IBuilder<DataSet>
    {
        private readonly List<Type> _numericTypes = new List<Type> { typeof(byte), typeof(sbyte), typeof(ushort), typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(float), typeof(double), typeof(decimal) };

        /// <summary>
        /// Build a dataset from an IBinnerDb object
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public DataSet Build(IBinnerDb db)
        {
            var dataSet = new DataSet("Datset");
            var partsTable = new DataTable("Parts");
            var partTypesTable = new DataTable("PartTypes");
            var projectsTable = new DataTable("Projects");
            dataSet.Locale = partsTable.Locale = partTypesTable.Locale = projectsTable.Locale = Thread.CurrentThread.CurrentCulture;
            dataSet.Tables.Add(partsTable);
            dataSet.Tables.Add(partTypesTable);
            dataSet.Tables.Add(projectsTable);

            // build the Parts table schema
            var partType = typeof(Part).GetExtendedType();
            foreach (var prop in partType.Properties)
                partsTable.Columns.Add(prop.Name, TranslateType(prop.Type));

            // build the PartTypes table schema
            var partTypesType = typeof(PartType).GetExtendedType();
            foreach (var prop in partTypesType.Properties)
                partTypesTable.Columns.Add(prop.Name, TranslateType(prop.Type));

            // build the Projects table schema
            var projectsType = typeof(Project).GetExtendedType();
            foreach (var prop in projectsType.Properties)
                projectsTable.Columns.Add(prop.Name, TranslateType(prop.Type));

            // populate the Parts table
            foreach (var entry in db.Parts)
            {
                var row = partsTable.NewRow();
                foreach (var prop in partType.Properties)
                {
                    var dataType = partsTable.Columns[prop.Name]?.DataType;
                    if (dataType != null)
                        row[prop.Name] = TranslateValue(entry.GetPropertyValue(prop), dataType, prop.Type);
                }

                partsTable.Rows.Add(row);
            }

            // populate the PartTypes table
            foreach (var entry in db.PartTypes)
            {
                var row = partTypesTable.NewRow();
                foreach (var prop in partTypesType.Properties)
                {
                    var dataType = partTypesTable.Columns[prop.Name]?.DataType;
                    if (dataType != null)
                        row[prop.Name] = TranslateValue(entry.GetPropertyValue(prop), dataType, prop.Type);
                }

                partTypesTable.Rows.Add(row);
            }

            // populate the Projects table
            foreach (var entry in db.Projects)
            {
                var row = projectsTable.NewRow();
                foreach (var prop in projectsType.Properties)
                {
                    var dataType = projectsTable.Columns[prop.Name]?.DataType;
                    if (dataType != null)
                        row[prop.Name] = TranslateValue(entry.GetPropertyValue(prop), dataType, prop.Type);
                }

                projectsTable.Rows.Add(row);
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
                // join collections
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
            if (extendedType.IsCollection)
                translatedType = extendedType.ElementType;
            if (_numericTypes.Contains(extendedType.UnderlyingType))
                translatedType = typeof(double);
            if (extendedType.UnderlyingType == typeof(TimeSpan))
                translatedType = typeof(string);
            return Nullable.GetUnderlyingType(translatedType) ?? translatedType;
        }
    }
}
