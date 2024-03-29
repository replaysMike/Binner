﻿using Binner.Global.Common;
using Binner.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Handles data import operations
    /// </summary>
    public class DataExporter
    {
        private readonly IStorageProvider _storageProvider;

        public DataExporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Export Binner database to multiple streams, one for each table.
        /// </summary>
        /// <param name="exportFormat">The Binner database to export</param>
        /// <param name="userContext"></param>
        public async Task<IDictionary<StreamName, Stream>> ExportAsync(string exportFormat, IUserContext? userContext)
        {
            var database = await _storageProvider.GetDatabaseAsync(userContext);
            switch (exportFormat.ToLower())
            {
                case "csv":
                    var csvExporter = new CsvDataExporter();
                    return csvExporter.Export(database);
                case "excel":
                    var excelExporter = new ExcelDataExporter();
                    return excelExporter.Export(database);
                case "sql":
                    var sqlExporter = new SqlDataExporter();
                    return sqlExporter.Export(database);
                default:
                    throw new ArgumentException($"Unknown format '{exportFormat}'");
            }
        }
    }
}
