using Binner.Common.Models;
using Binner.Common.StorageProviders;
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
            dataSet.Locale = partsTable.Locale = partTypesTable.Locale = Thread.CurrentThread.CurrentCulture;
            dataSet.Tables.Add(partsTable);
            dataSet.Tables.Add(partTypesTable);

            // build the Parts table schema
            var partType = typeof(Part).GetExtendedType();
            foreach (var prop in partType.Properties)
                partsTable.Columns.Add(prop.Name, prop.Type);

            // build the PartTypes table schema
            var partTypesType = typeof(PartType).GetExtendedType();
            foreach (var prop in partTypesType.Properties)
                partTypesTable.Columns.Add(prop.Name, prop.Type);

            // populate the Parts table
            foreach (var part in db.Parts)
            {
                var row = partsTable.NewRow();
                foreach (var prop in partType.Properties)
                    row[prop.Name] = part.GetPropertyValue(prop);
                partsTable.Rows.Add(row);
            }

            // populate the PartTypes table
            foreach (var part in db.PartTypes)
            {
                var row = partTypesTable.NewRow();
                foreach (var prop in partTypesType.Properties)
                    row[prop.Name] = part.GetPropertyValue(prop);
                partTypesTable.Rows.Add(row);
            }
            return dataSet;
        }
    }
}
