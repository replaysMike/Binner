using Binner.Common.Configuration;
using Binner.Common.IO;
using Binner.Common.Models;
using Binner.Model.Common;
using ICSharpCode.SharpZipLib.Checksum;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class StoredFileService : IStoredFileService
    {
        private IStorageProvider _storageProvider;
        private RequestContextAccessor _requestContext;
        private StorageProviderConfiguration _configuration;

        public StoredFileService(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor, StorageProviderConfiguration configuration)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _configuration = configuration;
        }

        public async Task<ICollection<StoredFile>> UploadFilesAsync(ICollection<UserUploadedFile> files)
        {
            if (string.IsNullOrEmpty(_configuration.UserUploadedFilesPath))
                throw new BinnerConfigurationException($"No 'StorageProviderConfiguration.UserUploadedFilesPath' value is provided in the configuration. Please set the path you would like to store files in appsettings.json.");
            if (!files.Any()) return new List<StoredFile>();

            var userContext = _requestContext.GetUserContext();
            var storedFiles = new List<StoredFile>();
            var partId = files.FirstOrDefault()?.PartId;
            Part? part = null;
            if (partId != null && partId > 0)
            {
                part = await _storageProvider.GetPartAsync(partId.Value, userContext);
            }

            if (part == null)
                throw new Exception($"Could not find part with id '{partId}'!");

            // group by extension
            foreach (var file in files)
            {
                var pathToFile = GenerateFilename(file.Filename, part, file.StoredFileType);
                var storedFile = await AddStoredFileAsync(new StoredFile
                {
                    StoredFileType = file.StoredFileType,
                    PartId = part.PartId,
                    FileName = Path.GetFileName(pathToFile),
                    OriginalFileName = file.Filename,
                    FileLength = (int)file.Stream.Length,
                    Crc32 = Checksum.Compute(file.Stream)
                });
                storedFiles.Add(storedFile);

                try
                {
                    // save the file to disk
                    if (!Directory.Exists(pathToFile))
                    {
                        var dir = Path.GetDirectoryName(pathToFile);
                        if (dir != null)
                            Directory.CreateDirectory(dir);
                    }

                    using (var fileStream = File.Create(pathToFile))
                    {
                        file.Stream.Position = 0;
                        file.Stream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                }
                catch (IOException)
                {
                    throw;
                }
            }

            return storedFiles;
        }

        public string GetStoredFilePath(StoredFileType storedFileType)
        {
            if (string.IsNullOrEmpty(_configuration.UserUploadedFilesPath))
                throw new Exception("No UserUploadedFilesPath set in the configuration");
            return Path.Combine(_configuration.UserUploadedFilesPath, storedFileType.ToString());
        }

        private string GenerateFilename(string originalFilename, Part part, StoredFileType storedFileType)
        {
            if (string.IsNullOrEmpty(part.PartNumber))
                throw new Exception("No part number specified");
            var extension = Path.GetExtension(originalFilename);
            var filename = $"{part.PartNumber.Replace("/", "_")}-{storedFileType}{extension}";
            var path = GetStoredFilePath(storedFileType);
            var pathToFile = Path.Combine(path, filename);
            var i = 1;
            while (File.Exists(pathToFile) && i < 10000)
            {
                filename = $"{part.PartNumber.Replace("/", "_")}-{storedFileType}-{i}{extension}";
                pathToFile = Path.Combine(path, filename);
                i++;
            }
            return pathToFile;
        }

        public async Task<StoredFile> AddStoredFileAsync(StoredFile storedFile)
        {
            return await _storageProvider.AddStoredFileAsync(storedFile, _requestContext.GetUserContext());
        }

        public async Task<bool> DeleteStoredFileAsync(StoredFile storedFile)
        {
            var userContext = _requestContext.GetUserContext();
            var entity = await _storageProvider.GetStoredFileAsync(storedFile.StoredFileId, userContext);
            if (entity != null)
            {
                var success = await _storageProvider.DeleteStoredFileAsync(entity, userContext);
                if (success)
                {
                    var path = GetStoredFilePath(entity.StoredFileType);
                    var pathToFile = Path.Combine(path, entity.FileName);
                    if (File.Exists(pathToFile))
                        File.Delete(pathToFile);
                }
                return success;
            }
            return false;
        }

        public async Task<StoredFile?> GetStoredFileAsync(long storedFileId)
        {
            return await _storageProvider.GetStoredFileAsync(storedFileId, _requestContext.GetUserContext());
        }

        public async Task<StoredFile?> GetStoredFileAsync(string filename)
        {
            return await _storageProvider.GetStoredFileAsync(filename, _requestContext.GetUserContext());
        }

        public async Task<ICollection<StoredFile>> GetStoredFilesAsync(long partId, StoredFileType? fileType)
        {
            return await _storageProvider.GetStoredFilesAsync(partId, fileType, _requestContext.GetUserContext());
        }

        public async Task<ICollection<StoredFile>> GetStoredFilesAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetStoredFilesAsync(request, _requestContext.GetUserContext());
        }

        public async Task<StoredFile> UpdateStoredFileAsync(StoredFile storedFile)
        {
            return await _storageProvider.UpdateStoredFileAsync(storedFile, _requestContext.GetUserContext());
        }
    }
}
