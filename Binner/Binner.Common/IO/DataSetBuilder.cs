using Binner.Common.Models;
using Binner.Common.StorageProviders;
using System;
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
                partsTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.Type) ?? prop.Type);

            // build the PartTypes table schema
            var partTypesType = typeof(PartType).GetExtendedType();
            foreach (var prop in partTypesType.Properties)
                partTypesTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.Type) ?? prop.Type);

            // build the Projects table schema
            var projectsType = typeof(Project).GetExtendedType();
            foreach (var prop in projectsType.Properties)
                projectsTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.Type) ?? prop.Type);

            // populate the Parts table
            foreach (var part in db.Parts)
            {
                var row = partsTable.NewRow();
                foreach (var prop in partType.Properties)
                    row[prop.Name] = part.GetPropertyValue(prop) ?? DBNull.Value;
                partsTable.Rows.Add(row);
            }

            // populate the PartTypes table
            foreach (var part in db.PartTypes)
            {
                var row = partTypesTable.NewRow();
                foreach (var prop in partTypesType.Properties)
                    row[prop.Name] = part.GetPropertyValue(prop) ?? DBNull.Value;
                partTypesTable.Rows.Add(row);
            }

            // populate the Projects table
            foreach (var part in db.Projects)
            {
                var row = projectsTable.NewRow();
                foreach (var prop in projectsType.Properties)
                    row[prop.Name] = part.GetPropertyValue(prop) ?? DBNull.Value;
                projectsTable.Rows.Add(row);
            }

            return dataSet;
        }
    }
}
