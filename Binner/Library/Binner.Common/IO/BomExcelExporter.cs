using Binner.Model.Responses;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Binner.Common.IO
{
    public class BomExcelExporter
    {
        public Stream Export(BomResponse data)
        {
            var stream = new MemoryStream();
            IWorkbook workbook = new XSSFWorkbook();
            var styles = CreateStyles(workbook);

            var rowIndex = 0;
            var columnIndex = 0;

            // All parts tab
            var allsheet = workbook.CreateSheet("All Parts");
            allsheet.DefaultColumnWidth = 130;

            // header
            rowIndex = 0;
            var allHeaderRow = allsheet.CreateRow(rowIndex);
            rowIndex++;
            columnIndex = 0;
            CreateCell(allHeaderRow, "Pcb", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "OutOfStock", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "PartNumber", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Mfr Part", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Part Type", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Cost", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "TotalCost", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Currency", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Qty Required", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Qty In Stock", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Lead Time", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Reference Id", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Description", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Note", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "SchematicReferenceId", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "CustomDescription", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Package", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Mounting Type", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "DatasheetUrl", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "ProductUrl", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Location", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "BinNumber", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "BinNumber2", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "ExtensionValue1", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "ExtensionValue2", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "DigiKey Part", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Mouser Part", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Arrow Part", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Tme Part", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Footprint Name", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Symbol Name", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Keywords", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Custom", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Product Status", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Base Product Number", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Series", "header", styles, ref columnIndex);
            CreateCell(allHeaderRow, "Rohs Status", "header", styles, ref columnIndex);
            /*
             "Package",
                "Mounting Type",
                "DatasheetUrl",
                "ProductUrl",
                "Location",
                "BinNumber",
                "BinNumber2",
                "ExtensionValue1",
                "ExtensionValue2",
                "DigiKey Part",
                "Mouser Part",
                "Arrow Part",
                "Tme Part",
                "Footprint Name",
                "Symbol Name",
                "Keywords",
                "Custom",
                "Product Status",
                "Base Product Number",
                "Series",
                "Rohs Status",
             */

            foreach (var part in data.Parts.OrderBy(x => x.PcbId).ThenBy(x => x.PartName))
            {
                var row = allsheet.CreateRow(rowIndex);
                rowIndex++;
                columnIndex = 0;
                var outOfStock = part.Quantity > (part.Part?.Quantity ?? part.QuantityAvailable);
                var rowStyle = outOfStock ? "outofstock" : null;

                SetCellValue(row.CreateCell(columnIndex), data.Pcbs.Where(x => x.PcbId == part.PcbId).Select(x => x.Name).FirstOrDefault(), styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), outOfStock ? 1 : 0, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.PartName, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.ManufacturerPartNumber, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.PartType, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.Cost ?? part.Cost, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Quantity * (part.Part?.Cost ?? part.Cost), styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.Currency ?? part.Currency, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Quantity, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.Quantity ?? part.QuantityAvailable, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.LeadTime ?? part.Part?.LeadTime, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.ReferenceId, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.Description, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Notes, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.SchematicReferenceId, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.CustomDescription, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.PackageType, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.MountingType, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.DatasheetUrl, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.ProductUrl, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.Location, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.BinNumber, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.BinNumber2, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.ExtensionValue1, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.ExtensionValue2, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.DigiKeyPartNumber, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.MouserPartNumber, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.ArrowPartNumber, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.TmePartNumber, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.FootprintName, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.SymbolName, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Part?.Keywords, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), string.Join(", ", part.Part?.CustomFields.Select(x => $"{x.Field}={x.Value}").ToList() ?? new List<string>()), styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.ProductStatus, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.BaseProductNumber, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.Series, styles, rowStyle); columnIndex++;
                SetCellValue(row.CreateCell(columnIndex), part.RohsStatus, styles, rowStyle); columnIndex++;
            }
            for (var i = 0; i < 10; i++)
            {
                //usheet.AutoSizeColumn(i);
                allsheet.SetColumnWidth(i, 20 * 256);
            }


            // Unassociated parts tab
            if (data.Parts.Any(x => x.PcbId == null))
            {
                var usheet = workbook.CreateSheet("Unassociated");
                usheet.DefaultColumnWidth = 130;

                // header
                rowIndex = 0;
                var headerRow = usheet.CreateRow(rowIndex);
                rowIndex++;
                columnIndex = 0;
                CreateCell(headerRow, "OutOfStock", "header", styles, ref columnIndex);
                CreateCell(headerRow, "PartNumber", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Mfr Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Part Type", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Cost", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Total Cost", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Currency", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Qty Required", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Qty In Stock", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Lead Time", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Reference Id", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Description", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Note", "header", styles, ref columnIndex);
                CreateCell(headerRow, "SchematicReferenceId", "header", styles, ref columnIndex);
                CreateCell(headerRow, "CustomDescription", "header", styles, ref columnIndex);

                foreach (var part in data.Parts.Where(x => x.PcbId == null).OrderBy(x => x.PartName))
                {
                    var row = usheet.CreateRow(rowIndex);
                    rowIndex++;
                    columnIndex = 0;
                    var outOfStock = part.Quantity > (part.Part?.Quantity ?? part.QuantityAvailable);
                    var rowStyle = outOfStock ? "outofstock" : null;
                    SetCellValue(row.CreateCell(columnIndex), outOfStock ? 1 : 0, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.PartName, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ManufacturerPartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.PartType, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Cost ?? part.Cost, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), (part.Part?.Cost ?? part.Cost) * part.Quantity, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Currency ?? part.Currency, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Quantity, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Quantity ?? part.QuantityAvailable, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.LeadTime ?? part.Part?.LeadTime, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.ReferenceId, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Description, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Notes, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.SchematicReferenceId, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.CustomDescription, styles, rowStyle); columnIndex++;
                }
                for (var i = 0; i < 10; i++)
                {
                    //usheet.AutoSizeColumn(i);
                    usheet.SetColumnWidth(i, 20 * 256);
                }
            }


            // By PCB tab
            foreach (var pcb in data.Pcbs.OrderBy(x => x.Name))
            {
                var sheet = workbook.CreateSheet(pcb.Name);
                sheet.DefaultColumnWidth = 130;
                // header
                rowIndex = 0;
                var headerRow = sheet.CreateRow(rowIndex);
                rowIndex++;
                columnIndex = 0;
                CreateCell(headerRow, "OutOfStock", "header", styles, ref columnIndex);
                CreateCell(headerRow, "PartNumber", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Mfr Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Part Type", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Cost", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Total Cost", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Currency", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Qty Required", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Qty In Stock", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Lead Time", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Reference Id", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Description", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Note", "header", styles, ref columnIndex);
                CreateCell(headerRow, "SchematicReferenceId", "header", styles, ref columnIndex);
                CreateCell(headerRow, "CustomDescription", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Package", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Mounting Type", "header", styles, ref columnIndex);
                CreateCell(headerRow, "DatasheetUrl", "header", styles, ref columnIndex);
                CreateCell(headerRow, "ProductUrl", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Location", "header", styles, ref columnIndex);
                CreateCell(headerRow, "BinNumber", "header", styles, ref columnIndex);
                CreateCell(headerRow, "BinNumber2", "header", styles, ref columnIndex);
                CreateCell(headerRow, "ExtensionValue1", "header", styles, ref columnIndex);
                CreateCell(headerRow, "ExtensionValue2", "header", styles, ref columnIndex);
                CreateCell(headerRow, "DigiKey Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Mouser Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Arrow Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Tme Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Footprint Name", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Symbol Name", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Keywords", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Custom", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Product Status", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Base Product Number", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Series", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Rohs Status", "header", styles, ref columnIndex);

                foreach (var part in data.Parts.Where(x => x.PcbId == pcb.PcbId).OrderBy(x => x.PartName))
                {
                    var row = sheet.CreateRow(rowIndex);
                    rowIndex++;
                    columnIndex = 0;
                    var outOfStock = part.Quantity > (part.Part?.Quantity ?? part.QuantityAvailable);
                    var rowStyle = outOfStock ? "outofstock" : null;
                    SetCellValue(row.CreateCell(columnIndex), outOfStock ? 1 : 0, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.PartName, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ManufacturerPartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.PartType, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Cost ?? part.Cost, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), (part.Part?.Cost ?? part.Cost) * part.Quantity, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Currency ?? part.Currency, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Quantity, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Quantity ?? part.QuantityAvailable, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.LeadTime ?? part.Part?.LeadTime, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.ReferenceId, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Description, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Notes, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.SchematicReferenceId, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.CustomDescription, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.PackageType, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.MountingType, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.DatasheetUrl, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ProductUrl, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Location, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.BinNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.BinNumber2, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ExtensionValue1, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ExtensionValue2, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.DigiKeyPartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.MouserPartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ArrowPartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.TmePartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.FootprintName, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.SymbolName, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Keywords, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), string.Join(", ", part.Part?.CustomFields.Select(x => $"{x.Field}={x.Value}").ToList() ?? new List<string>()), styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.ProductStatus, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.BaseProductNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Series, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.RohsStatus, styles, rowStyle); columnIndex++;
                }
                for (var i = 0; i < 10; i++)
                {
                    //sheet.AutoSizeColumn(i);
                    sheet.SetColumnWidth(i, 20 * 256);
                }
            }

            // Out of Stock parts tab
            // create a sheet for all out of stock items
            if (data.Parts.Any(x => x.Quantity > (x.Part?.Quantity ?? x.QuantityAvailable)))
            {
                var osheet = workbook.CreateSheet("Out Of Stock");
                osheet.DefaultColumnWidth = 130;

                // header
                rowIndex = 0;
                var headerRow = osheet.CreateRow(rowIndex);
                rowIndex++;
                columnIndex = 0;
                CreateCell(headerRow, "Pcb", "header", styles, ref columnIndex);
                CreateCell(headerRow, "PartNumber", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Mfr Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Part Type", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Cost", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Total Cost", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Currency", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Qty Required", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Qty In Stock", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Lead Time", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Reference Id", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Description", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Note", "header", styles, ref columnIndex);
                CreateCell(headerRow, "SchematicReferenceId", "header", styles, ref columnIndex);
                CreateCell(headerRow, "CustomDescription", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Package", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Mounting Type", "header", styles, ref columnIndex);
                CreateCell(headerRow, "DatasheetUrl", "header", styles, ref columnIndex);
                CreateCell(headerRow, "ProductUrl", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Location", "header", styles, ref columnIndex);
                CreateCell(headerRow, "BinNumber", "header", styles, ref columnIndex);
                CreateCell(headerRow, "BinNumber2", "header", styles, ref columnIndex);
                CreateCell(headerRow, "ExtensionValue1", "header", styles, ref columnIndex);
                CreateCell(headerRow, "ExtensionValue2", "header", styles, ref columnIndex);
                CreateCell(headerRow, "DigiKey Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Mouser Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Arrow Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Tme Part", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Footprint Name", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Symbol Name", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Keywords", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Custom", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Product Status", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Base Product Number", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Series", "header", styles, ref columnIndex);
                CreateCell(headerRow, "Rohs Status", "header", styles, ref columnIndex);

                foreach (var part in data.Parts.Where(x => x.Quantity > (x.Part?.Quantity ?? x.QuantityAvailable)).OrderBy(x => x.PcbId).ThenBy(x => x.PartName))
                {
                    var row = osheet.CreateRow(rowIndex);
                    rowIndex++;
                    columnIndex = 0;
                    var rowStyle = "outofstock";
                    SetCellValue(row.CreateCell(columnIndex), data.Pcbs.Where(x => x.PcbId == part.PcbId).Select(x => x.Name).FirstOrDefault(), styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.PartName, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ManufacturerPartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.PartType, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Cost ?? part.Cost, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), (part.Part?.Cost ?? part.Cost) * part.Quantity, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Currency ?? part.Currency, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Quantity, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Quantity ?? part.QuantityAvailable, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), "", styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.ReferenceId, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Description, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Notes, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.SchematicReferenceId, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.CustomDescription, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.PackageType, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.MountingType, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.DatasheetUrl, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ProductUrl, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Location, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.BinNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.BinNumber2, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ExtensionValue1, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ExtensionValue2, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.DigiKeyPartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.MouserPartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.ArrowPartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.TmePartNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.FootprintName, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.SymbolName, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Part?.Keywords, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), string.Join(", ", part.Part?.CustomFields.Select(x => $"{x.Field}={x.Value}").ToList() ?? new List<string>()), styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.ProductStatus, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.BaseProductNumber, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.Series, styles, rowStyle); columnIndex++;
                    SetCellValue(row.CreateCell(columnIndex), part.RohsStatus, styles, rowStyle); columnIndex++;
                }
                for (var i = 0; i < 10; i++)
                {
                    //osheet.AutoSizeColumn(i);
                    osheet.SetColumnWidth(i, 20 * 256);
                }
            }

            workbook.Write(stream, true);
            var bytes = stream.ToArray();
            stream.Dispose();
            return new MemoryStream(bytes, false);
        }

        private IDictionary<string, ICellStyle> CreateStyles(IWorkbook workbook)
        {
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.Alignment = HorizontalAlignment.Center;
            var headerFont = (XSSFFont)workbook.CreateFont();
            //var xssFont = new XSSFFont(new CT_Font());
            headerFont.IsBold = true;
            headerFont.Color = HSSFColor.White.Index;
            headerFont.FontHeightInPoints = 14;
            headerStyle.SetFont(headerFont);

            headerStyle.FillForegroundColor = HSSFColor.SkyBlue.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.TopBorderColor = HSSFColor.Black.Index;
            headerStyle.BottomBorderColor = HSSFColor.Black.Index;
            var dateStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd HH:mm:ss");

            var outOfStockStyle = workbook.CreateCellStyle();
            var oosFont = (XSSFFont)workbook.CreateFont();
            //var xssFont = new XSSFFont(new CT_Font());
            oosFont.IsBold = true;
            oosFont.Color = HSSFColor.Red.Index;
            oosFont.FontHeightInPoints = 11;
            outOfStockStyle.SetFont(oosFont);

            return new Dictionary<string, ICellStyle> {
                { "header", headerStyle },
                { "date", dateStyle },
                { "outofstock", outOfStockStyle },
            };
        }

        private void CreateCell(IRow headerRow, string name, string styleName, IDictionary<string, ICellStyle> styles, ref int columnIndex)
        {
            var c = headerRow.CreateCell(columnIndex);
            c.SetCellValue(name);
            c.CellStyle = styles[styleName];
            columnIndex++;
        }

        private void SetCellValue(ICell cell, object? value, IDictionary<string, ICellStyle> styles, string? styleName = null)
        {
            if (value == null) return;
            if(!string.IsNullOrEmpty(styleName) && styles.ContainsKey(styleName))
                cell.CellStyle = styles[styleName];
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
                    throw new InvalidOperationException($"Unknown excel export data type format: {value?.GetType().Name}");
            }
        }
    }
}
