using Binner.Common;
using Binner.Common.IO;
using Binner.Common.Models;
using Binner.Model.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class DataController : ControllerBase
    {
        private readonly string BinnerExportFilename = $"binner-export-{DateTime.Now.ToString("yyyy-MM-dd")}.zip";
        private readonly IStorageProvider _storageProvider;
        private RequestContextAccessor _requestContext;

        public DataController(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
        }

        /// <summary>
        /// Import custom user data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportAsync(List<IFormFile> files)
        {
            var importer = new DataImporter(_storageProvider);
            var uploadFiles = new List<UploadFile>();
            foreach(var file in files)
            {
                var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;
                var uploadFile = new UploadFile(file.FileName, stream);
                uploadFiles.Add(uploadFile);
            }
            var importResult = await importer.ImportAsync(uploadFiles, _requestContext.GetUserContext());
            
            // cleanup
            foreach(var file in uploadFiles)
                file.Stream?.Dispose();

            return Ok(importResult);
        }

        /// <summary>
        /// Export user data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("export")]
        public async Task<IActionResult> ExportAsync([FromQuery] ExportRequest request)
        {
            if (string.IsNullOrEmpty(request.ExportFormat))
                return BadRequest($"No export format specified");

            var exporter = new DataExporter(_storageProvider);
            try
            {
                return ExportToFile(await exporter.ExportAsync(request.ExportFormat.ToLower(), _requestContext.GetUserContext()));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error exporting data: {ex.Message}");
            }
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
