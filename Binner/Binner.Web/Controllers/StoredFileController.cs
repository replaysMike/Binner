using AnyMapper;
using Binner.Common;
using Binner.Common.IO;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Binner.Services;
using Binner.Services.IO.Printing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class StoredFileController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;
        private readonly IStoredFileService _storedFileService;
        private readonly IUserService<User> _userService;

        public StoredFileController(ILogger<ProjectController> logger, WebHostServiceConfiguration config, IPartService partService, IStoredFileService storedFileService, IUserService<User> userService)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _storedFileService = storedFileService;
            _userService = userService;
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
        /// Check if an existing stored file exists on disk
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet("exists")]
        public async Task<IActionResult> GetStoredFileExistsAsync([FromQuery] string fileName)
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
                return Ok();
            return NotFound();
        }

        /// <summary>
        /// Get an existing user uploaded file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet("local")]
        public async Task<IActionResult> GetStoredFileContentAsync([FromQuery] string fileName, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(fileName)) return NotFound();
            if (string.IsNullOrEmpty(token)) return ValidationProblem("Invalid token.");
            var userContext = await _userService.ValidateUserImageToken(token);
            if (userContext == null) return ValidationProblem("Invalid token.");
            System.Threading.Thread.CurrentPrincipal = new TokenPrincipal(userContext, token);

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
        [AllowAnonymous]
        [HttpGet("preview")]
        public async Task<IActionResult> GetStoredFilePreviewAsync([FromQuery] StoredFilePreviewRequest request)
        {
            var userContext = await _userService.ValidateUserImageToken(request.Token ?? string.Empty);
            if (userContext == null) return GetInvalidTokenImage();
            System.Threading.Thread.CurrentPrincipal = new TokenPrincipal(userContext, request.Token);

            if (string.IsNullOrEmpty(request.Filename)) return NotFound();
            var storedFile = await _storedFileService.GetStoredFileAsync(request.Filename);
            if (storedFile == null) return NotFound();

            // read the file contents
            new FileExtensionContentTypeProvider().TryGetContentType(request.Filename, out var contentType);
            if (string.IsNullOrEmpty(contentType))
                return BadRequest($"Could not determine content type for file '{request.Filename}'");
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
            var pathToFile = Path.Combine(path, request.Filename);
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
        public async Task<IActionResult> DeleteStoredFileAsync(DeleteStoredFileRequest request)
        {
            var isDeleted = await _storedFileService.DeleteStoredFileAsync(new StoredFile
            {
                StoredFileId = request.StoredFileId
            });
            return Ok(isDeleted);
        }

        private FileStreamResult GetInvalidTokenImage()
        {
            var image = new BlankImage(300, 100, Color.White, Color.Red, "Invalid Image Token!\nYou may need to re-login.");
            var stream = new MemoryStream();
            image.Image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "image/png");
        }
    }
}
