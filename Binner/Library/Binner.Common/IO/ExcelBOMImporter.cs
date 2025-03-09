using Binner.Global.Common;
using Binner.Model;
using NPOI.SS.UserModel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Imports data from Excel Open XML Format 2007+ (XLSX)
    /// </summary>
    public class ExcelBOMImporter : BOMImporter
    {
        public ExcelBOMImporter(IStorageProvider storageProvider)
            : base(storageProvider)
        {
        }

        public async Task<ImportResult> ImportAsync(Project project, Stream stream, IUserContext? userContext)
        {
            var result = new ImportResult();
            try
            {
                stream.Position = 0;
                var workbook = WorkbookFactory.Create(stream);
                var worksheet = workbook.GetSheetAt(0);
                if (worksheet != null)
                {
                    ImportSheet(project, userContext, worksheet, result);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            result.Success = !result.Errors.Any();

            return result;
        }

    }
}