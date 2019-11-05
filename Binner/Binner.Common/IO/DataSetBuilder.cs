using Binner.Common.Models;
using Binner.Common.StorageProviders;
using System;
using System.Collections;
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
                    row[prop.Name] = TranslateValue(entry.GetPropertyValue(prop), prop.Type);
                partsTable.Rows.Add(row);
            }

            // populate the PartTypes table
            foreach (var entry in db.PartTypes)
            {
                var row = partTypesTable.NewRow();
                foreach (var prop in partTypesType.Properties)
                    row[prop.Name] = TranslateValue(entry.GetPropertyValue(prop), prop.Type);
                partTypesTable.Rows.Add(row);
            }

            // populate the Projects table
            foreach (var entry in db.Projects)
            {
                var row = projectsTable.NewRow();
                foreach (var prop in projectsType.Properties)
                    row[prop.Name] = TranslateValue(entry.GetPropertyValue(prop), prop.Type);
                projectsTable.Rows.Add(row);
            }

            return dataSet;
        }

        private object TranslateValue(object val, Type type)
        {
            var newVal = val;
            var extendedType = type.GetExtendedType();
            if (extendedType.IsCollection)
            {
                // join collections
                newVal = string.Join(",", (ICollection<string>)val);
            }
            return newVal ?? Activator.CreateInstance(Nullable.GetUnderlyingType(type) ?? type);
        }

        private Type TranslateType(Type type)
        {
            Type translatedType = type;
            var extendedType = type.GetExtendedType();
            if (extendedType.IsCollection)
            {
                translatedType = extendedType.ElementType;
            }
            return Nullable.GetUnderlyingType(translatedType) ?? translatedType;
        }
    }
}
