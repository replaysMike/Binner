using Binner.Common.Models;
using Binner.Model;
using Binner.Model.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Handles data import operations
    /// </summary>
    public class DataImporter : IDataImporter
    {
        private readonly IStorageProvider _storageProvider;

        public DataImporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Import Binner user data
        /// </summary>
        /// <param name="files"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        public async Task<ImportResult> ImportAsync(IEnumerable<UploadFile> files, UserContext userContext)
        {
            // group by extension
            var filesByExtension = files.GroupBy(x => Path.GetExtension(x.Filename));
            foreach(var group in filesByExtension)
            {
                var extension = group.Key;
                switch (extension.ToLower())
                {
                    case ".csv":
                        var csvImporter = new CsvDataImporter(_storageProvider);
                        return await csvImporter.ImportAsync(group.ToList(), userContext);
                    case ".xls":
                    case ".xlsx":
                    case ".xlsm":
                    case ".xlsb":
                        var excelImporter = new ExcelDataImporter(_storageProvider);
                        return await excelImporter.ImportAsync(group.ToList(), userContext);
                    case ".sql":
                        var sqlImporter = new SqlDataImporter(_storageProvider);
                        return await sqlImporter.ImportAsync(group.ToList(), userContext);
                }
            }
            return new ImportResult();
        }

        /// <summary>
        /// Import Binner user data
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="stream"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ImportResult> ImportAsync(string filename, Stream stream, UserContext userContext)
        {
            var extension = Path.GetExtension(filename);
            switch (extension.ToLower())
            {
                case ".csv":
                    var csvImporter = new CsvDataImporter(_storageProvider);
                    return await csvImporter.ImportAsync(filename, stream, userContext);
                case ".xls":
                case ".xlsx":
                case ".xlsm":
                case ".xlsb":
                    var excelImporter = new ExcelDataImporter(_storageProvider);
                    return await excelImporter.ImportAsync(filename, stream, userContext);
                case ".sql":
                    var sqlImporter = new SqlDataImporter(_storageProvider);
                    return await sqlImporter.ImportAsync(filename, stream, userContext);
                default:
                    throw new ArgumentException($"Unhandled file type with extension: '{extension}'");
            }
        }
    }
}
