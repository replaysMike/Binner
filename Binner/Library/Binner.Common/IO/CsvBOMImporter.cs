using Binner.Global.Common;
using Binner.Model;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public class CsvBOMImporter : BOMImporter
    {

        public CsvBOMImporter(IStorageProvider storageProvider)
            : base(storageProvider)
        {
        }

        private void CreateCell(IRow CurrentRow, int CellIndex, string Value)
        {
            ICell Cell = CurrentRow.CreateCell(CellIndex);
            Cell.SetCellValue(Value);
        }

        public async Task<ImportResult> ImportAsync(Project project, Stream stream, IUserContext? userContext)
        {
            var result = new ImportResult();
            try
            {
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var data = await reader.ReadToEndAsync();
                // remove line breaks
                data = data.Replace("\r", "");

                // Parse out rows
                var rows = SplitBoundaries(data, new char[] { '\n' });
                if (!rows.Any())
                {
                    result.Success = true;
                    result.Warnings.Add("No rows were found!");
                    return result;
                }

                // Convert to an NPOI workbook
                HSSFWorkbook workbook = new HSSFWorkbook();
                ISheet worksheet = workbook.CreateSheet("BOM");
                var rowNumber = 0;
                foreach (var row in rows)
                {
                    var rowData = SplitBoundaries(row, new char[] { ',' }, true);
                    IRow sheetRow = worksheet.CreateRow(rowNumber);
                    var columnNumber = 0;
                    foreach (var column in rowData)
                    {
                        CreateCell(sheetRow, columnNumber, column);
                        columnNumber++;
                    }
                    rowNumber++;
                }

                ImportSheet(project, userContext, worksheet, result);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            result.Success = !result.Errors.Any();

            return result;
        }

        private string[] SplitBoundaries(string data, char[] rowDelimiters, bool removeBoundary = false)
        {
            var rows = new List<string>();
            var quotes = new List<char> { '"', '\'' };
            var startPos = 0;
            var insideQuotes = false;
            var insideQuotesChar = '\0';
            for (var i = 0; i < data.Length; i++)
            {
                var c = data[i];
                if (quotes.Contains(c))
                {
                    if (!insideQuotes)
                    {
                        insideQuotes = true;
                        insideQuotesChar = c;
                    }
                    else if (c == insideQuotesChar)
                    {
                        insideQuotes = false;
                    }
                }
                if ((rowDelimiters.Any(x => x.Equals(c)) && !insideQuotes) || i == data.Length - 1)
                {
                    var row = data.Substring(startPos, i - startPos + 1 - (removeBoundary && !(i == data.Length - 1) ? 1 : 0));
                    if (!string.IsNullOrEmpty(row) && row.Length > (removeBoundary ? 0 : 1))
                        rows.Add(row);
                    startPos = i + 1;
                }
            }

            return rows.ToArray();
        }
    }
}