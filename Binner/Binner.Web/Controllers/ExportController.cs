using Binner.Common;
using Binner.Common.IO;
using Binner.Common.Models;
using Binner.Model.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class ExportController : ControllerBase
    {
        private readonly string BinnerExportFilename = $"binner-export-{DateTime.Now.ToString("yyyy-MM-dd")}.zip";
        private readonly IStorageProvider _storageProvider;
        private RequestContextAccessor _requestContext;

        public ExportController(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ExportAsync([FromQuery] ExportRequest request)
        {
            if(string.IsNullOrEmpty(request.ExportFormat))
                return BadRequest($"No export format specified");
            switch (request.ExportFormat.ToLower())
            {
                case "csv":
                    return await ExportCsvAsync(_requestContext.GetUserContext());
                case "excel":
                    return await ExportExcelAsync(_requestContext.GetUserContext());
                case "sql":
                    return await ExportSqlAsync(_requestContext.GetUserContext());
                default:
                    return BadRequest($"Unknown format '{request.ExportFormat}'");
            }
        }

        private async Task<IActionResult> ExportCsvAsync(IUserContext? userContext)
        {
            var exporter = new CsvDataExporter();
            var streams = exporter.Export(await _storageProvider.GetDatabaseAsync(userContext));
            return ExportToFile(streams);
        }

        private async Task<IActionResult> ExportExcelAsync(IUserContext? userContext)
        {
            var exporter = new ExcelDataExporter();
            var streams = exporter.Export(await _storageProvider.GetDatabaseAsync(userContext));
            return ExportToFile(streams);
        }

        private async Task<IActionResult> ExportSqlAsync(IUserContext? userContext)
        {
            var exporter = new SqlDataExporter();
            var streams = exporter.Export(await _storageProvider.GetDatabaseAsync(userContext));
            return ExportToFile(streams);
        }

        private IActionResult ExportToFile(IDictionary<StreamName, Stream> streams)
        {
            var zipStream = new MemoryStream();
            using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var stream in streams)
                {
                    // write stream to a buffer
                    var buffer = new byte[stream.Value.Length];
                    stream.Value.Read(buffer, 0, (int)stream.Value.Length);

                    // add the stream to the zipfile
                    var file = zipFile.CreateEntry($"{stream.Key.Name}.{stream.Key.FileExtension}");
                    using var fileStream = file.Open();
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
            zipStream.Seek(0, SeekOrigin.Begin);
            return File(zipStream, "application/octet-stream", BinnerExportFilename);
        }
    }
}
