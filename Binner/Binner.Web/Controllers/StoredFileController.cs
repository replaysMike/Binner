using AnyMapper;
using Binner.Common.Configuration;
using Binner.Common.IO;
using Binner.Common.Models;
using Binner.Common.Models.Requests;
using Binner.Common.Models.Responses;
using Binner.Common.Services;
using Binner.Model.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using NPOI.HPSF;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class StoredFileController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;
        private readonly IStoredFileService _storedFileService;

        public StoredFileController(ILogger<ProjectController> logger, WebHostServiceConfiguration config, IPartService partService, IStoredFileService storedFileService)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _storedFileService = storedFileService;
        }

        /// <summary>
        /// Get an existing user uploaded file
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(GetStoredFileRequest request)
        {
            var storedFile = await _storedFileService.GetStoredFileAsync(request.StoredFileId);
            if (storedFile == null) return NotFound();

            return Ok(storedFile);
        }

        /// <summary>
        /// Get an existing user uploaded file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet("local")]
        public async Task<IActionResult> GetStoredFileContentAsync([FromQuery] string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return NotFound();
            var storedFile = await _storedFileService.GetStoredFileAsync(fileName);
            if (storedFile == null) return NotFound();

            // read the file contents
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            if (string.IsNullOrEmpty(contentType))
                return BadRequest($"Could not determine content type for file '{fileName}'");
            var path = _storedFileService.GetStoredFilePath(storedFile.StoredFileType);
            var pathToFile = Path.Combine(path, fileName);
            if (System.IO.File.Exists(pathToFile))
            {
                var bytes = System.IO.File.ReadAllBytes(pathToFile);
                return File(bytes, contentType);
            }
            return NotFound();
        }

        /// <summary>
        /// Get a preview image of the stored file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet("preview")]
        public async Task<IActionResult> GetStoredFilePreviewAsync([FromQuery] string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return NotFound();
            var storedFile = await _storedFileService.GetStoredFileAsync(fileName);
            if (storedFile == null) return NotFound();

            // read the file contents
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            if (string.IsNullOrEmpty(contentType))
                return BadRequest($"Could not determine content type for file '{fileName}'");
            // return a default cover image for the following formats
            switch (contentType)
            {
                case "application/pdf":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                case "application/msword":
                case "application/text":
                    var pathToCoverImage = Path.Combine("./Resources", "datasheet.png");
                    if (System.IO.File.Exists(pathToCoverImage))
                    {
                        var bytes = System.IO.File.ReadAllBytes(pathToCoverImage);
                        return File(bytes, contentType);
                    }
                    // error, cover image not found
                    return NotFound();
            }

            // return the actual file if it's an image type
            var path = _storedFileService.GetStoredFilePath(storedFile.StoredFileType);
            var pathToFile = Path.Combine(path, fileName);
            if (System.IO.File.Exists(pathToFile))
            {
                var bytes = System.IO.File.ReadAllBytes(pathToFile);
                return File(bytes, contentType);
            }
            return NotFound();
        }

        /// <summary>
        /// Get an existing user uploaded file
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetStoredFilesAsync([FromQuery] PaginatedRequest request)
        {
            var storedFiles = await _storedFileService.GetStoredFilesAsync(request);
            var storedFilesResponse = Mapper.Map<ICollection<StoredFile>, ICollection<StoredFileResponse>>(storedFiles);
            return Ok(storedFilesResponse);
        }

        /// <summary>
        /// Create a new user uploaded file
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateStoredFileAsync([FromForm] UploadUserFilesRequest<IFormFile> request)
        {
            var acceptedMimeTypes = new string[] { "application/pdf","image/jpeg","image/png","image/svg+xml","image/webp","image/gif","application/msword","application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
            if (request.Files == null || request.Files.Count == 0)
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse(new Exception("No files were uploaded!")));

            foreach(var file in request.Files)
            {
                if (!acceptedMimeTypes.Contains(file.ContentType))
                    return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse(new Exception($"File name '{file.FileName}' with type '{file.ContentType}' not accepted!")));
            }

            var uploadFiles = new List<UserUploadedFile>();
            foreach (var file in request.Files)
            {
                var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;
                var uploadFile = new UserUploadedFile(file.FileName, stream, request.PartId, request.StoredFileType);
                uploadFiles.Add(uploadFile);
            }

            var storedFiles = await _storedFileService.UploadFilesAsync(uploadFiles);

            // cleanup
            foreach (var file in uploadFiles)
                file.Stream?.Dispose();

            return Ok(storedFiles);
        }

        /// <summary>
        /// Update an existing user uploaded file
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateStoredFileAsync(UpdateStoredFileRequest request)
        {
            var mappedStoredFile = Mapper.Map<UpdateStoredFileRequest, StoredFile>(request);
            var storedFile = await _storedFileService.UpdateStoredFileAsync(mappedStoredFile);
            return Ok(storedFile);
        }

        /// <summary>
        /// Delete an existing user uploaded file
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeletePartAsync(DeleteStoredFileRequest request)
        {
            var isDeleted = await _storedFileService.DeleteStoredFileAsync(new StoredFile
            {
                StoredFileId = request.StoredFileId
            });
            return Ok(isDeleted);
        }
    }
}
