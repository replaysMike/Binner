using Binner.Common.IO;
using Binner.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Services
{
    public interface IStoredFileService
    {
        /// <summary>
        /// Get the file path to a stored file type
        /// </summary>
        /// <param name="storedFileType"></param>
        /// <returns></returns>
        string GetStoredFilePath(StoredFileType storedFileType);

        /// <summary>
        /// Upload new files
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<ICollection<StoredFile>> UploadFilesAsync(ICollection<UserUploadedFile> files);

        /// <summary>
        /// Add a new user uploaded file
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<StoredFile> AddStoredFileAsync(StoredFile storedFile);

        /// <summary>
        /// Update an existing user uploaded file
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<StoredFile?> UpdateStoredFileAsync(StoredFile storedFile);

        /// <summary>
        /// Delete an existing user uploaded file
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<bool> DeleteStoredFileAsync(StoredFile storedFile);

        /// <summary>
        /// Get a user uploaded file
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<StoredFile?> GetStoredFileAsync(long storedFileId);

        /// <summary>
        /// Get a user uploaded file
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<StoredFile?> GetStoredFileAsync(string filename);

        /// <summary>
        /// Get user uploaded files
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        Task<ICollection<StoredFile>> GetStoredFilesAsync(long partId, StoredFileType? fileType);

        /// <summary>
        /// Get all user uploaded file
        /// </summary>
        /// <returns></returns>
        Task<ICollection<StoredFile>> GetStoredFilesAsync(PaginatedRequest request);

        /// <summary>
        /// Generate a new filename for stored file
        /// </summary>
        /// <param name="originalFilename"></param>
        /// <param name="part"></param>
        /// <param name="storedFileType"></param>
        /// <returns></returns>
        string GenerateFilename(string originalFilename, Part part, StoredFileType storedFileType);
    }
}
