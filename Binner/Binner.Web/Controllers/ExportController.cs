﻿using Binner.Common.IO;
using Binner.Common.Models;
using Binner.Common.StorageProviders;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class ExportController : ControllerBase
    {
        private readonly string BinnerExportFilename = $"binner-export-{DateTime.Now.ToString("yyyy-MM-dd")}.zip";
        private readonly ILogger<ProjectController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IStorageProvider _storageProvider;

        public ExportController(ILogger<ProjectController> logger, WebHostServiceConfiguration config, IStorageProvider storageProvider)
        {
            _logger = logger;
            _config = config;
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ExportAsync([FromQuery] ExportRequest request)
        {
            switch (request.ExportFormat.ToLower())
            {
                case "csv":
                    return await ExportCsvAsync();
                case "excel":
                    return await ExportExcelAsync();
                default:
                    return BadRequest($"Unknown format '{request.ExportFormat}'");
            }
        }

        private async Task<IActionResult> ExportCsvAsync()
        {
            var exporter = new CSVDataExporter();
            var streams = exporter.Export(await _storageProvider.GetDatabaseAsync());
            return ExportToFile(streams);
        }

        private async Task<IActionResult> ExportExcelAsync()
        {
            var exporter = new ExcelDataExporter();
            var streams = exporter.Export(await _storageProvider.GetDatabaseAsync());
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
