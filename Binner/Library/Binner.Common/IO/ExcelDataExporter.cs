using Binner.Common.StorageProviders;
using NPOI.HSSF.Util;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Binner.Common.IO
{
    /// <summary>
    /// Exports data to Excel Open XML Format 2007+ (XLSX)
    /// </summary>
    public class ExcelDataExporter : IDataExporter
    {
        public IDictionary<StreamName, Stream> Export(IBinnerDb db)
        {
            var streams = new Dictionary<StreamName, Stream>();
            var builder = new DataSetBuilder();
            var dataSet = builder.Build(db);

            var stream = new MemoryStream();
            IWorkbook workbook = new XSSFWorkbook();
            var styles = CreateStyles(workbook);

            foreach (DataTable table in dataSet.Tables)
            {
                ISheet sheet = workbook.CreateSheet(table.TableName);
                sheet.DefaultColumnWidth = 120;

                // insert header row
                var rowIndex = 0;
                IRow headerRow = sheet.CreateRow(rowIndex);
                rowIndex++;
                var columnIndex = 0;
                foreach (DataColumn dataColumn in table.Columns)
                {
                    var c = headerRow.CreateCell(columnIndex);
                    c.SetCellValue(dataColumn.ColumnName);
                    c.CellStyle = styles["header"];
                    columnIndex++;
                }
                // insert all the row data for table
                foreach (DataRow dataRow in table.Rows)
                {
                    IRow row = sheet.CreateRow(rowIndex);
                    rowIndex++;
                    columnIndex = 0;
                    foreach (var value in dataRow.ItemArray)
                    {
                        SetCellValue(row.CreateCell(columnIndex), value, styles);
                        columnIndex++;
                    }
                }
                for (var i = 0; i < table.Columns.Count; i++)
                    sheet.AutoSizeColumn(i);
            }
            workbook.Write(stream);
            var bytes = stream.ToArray();
            stream.Dispose();
            streams.Add(new StreamName("BinnerParts", "xlsx"), new MemoryStream(bytes, false));
            return streams;
        }

        private IDictionary<string, ICellStyle> CreateStyles(IWorkbook workbook)
        {
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.Alignment = HorizontalAlignment.Center;
            var xssFont = new XSSFFont(new CT_Font());
            xssFont.IsBold = true;
            headerStyle.SetFont(xssFont);
            headerStyle.FillForegroundColor = HSSFColor.LightBlue.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.TopBorderColor = HSSFColor.Black.Index;
            headerStyle.BottomBorderColor = HSSFColor.Black.Index;
            var dateStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd HH:mm:ss");
            return new Dictionary<string, ICellStyle> {
                { "header", headerStyle },
                { "date", dateStyle },
            };
        }

        private void SetCellValue(ICell cell, object value, IDictionary<string, ICellStyle> styles)
        {
            switch (value)
            {
                case string s:
                    cell.SetCellType(CellType.String);
                    cell.SetCellValue(s);
                    break;
                case byte d:
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(d);
                    break;
                case short d:
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(d);
                    break;
                case int d:
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(d);
                    break;
                case long d:
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(d);
                    break;
                case float d:
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(d);
                    break;
                case decimal d:
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue((double)d);
                    break;
                case double d:
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(d);
                    break;
                case DateTime d:
                    cell.CellStyle = styles["date"];
                    cell.SetCellValue(d);
                    break;
                case bool b:
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(b);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown excel export data type format: {value.GetType().Name}");
            }
        }
    }
}
