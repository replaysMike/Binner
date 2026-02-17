using Binner.Global.Common;
using Binner.Model;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public class CsvBOMImporter : BOMImporter
    {

        public CsvBOMImporter(IStorageProvider storageProvider)
            : base(storageProvider)
        {
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
                var rows = SplitBoundaries(data, ['\n']);
                if (!rows.Any())
                {
                    result.Success = true;
                    result.Warnings.Add($"No rows were found for project '{project.Name}'!");
                    return result;
                }

                // Convert to an NPOI workbook
                var workbook = new HSSFWorkbook();
                var worksheet = workbook.CreateSheet("BOM");
                var headerRow = worksheet.CreateRow(0);

                var rowNumber = 0;
                foreach (var row in rows)
                {
                    var rowData = SplitBoundaries(row, [',', ';'], true);   // KiCad CSV format uses comma (Schematic BOM) or semicolon (PCB BOM) as a delimiter
                    var sheetRow = worksheet.CreateRow(rowNumber);
                    PopulateRow(sheetRow, rowData);
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

        private void CreateCell(IRow CurrentRow, int CellIndex, string Value)
        {
            var Cell = CurrentRow.CreateCell(CellIndex);
            Cell.SetCellValue(Value);
        }

        private void PopulateRow(IRow row, string[] rowData)
        {
            var columnNumber = 0;
            foreach (var column in rowData)
            {
                CreateCell(row, columnNumber, column);
                columnNumber++;
            }

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
                    if (row != null)
                        rows.Add(row);
                    startPos = i + 1;
                }
            }

            return rows.ToArray();
        }
    }
}