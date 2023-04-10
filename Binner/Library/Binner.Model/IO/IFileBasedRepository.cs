namespace Binner.Model.IO
{
    public interface IFileBasedRepository
    {
        /// <summary>
        /// Store a physical file in the repository
        /// </summary>
        /// <param name="data">The file data to store</param>
        /// <param name="sequenceId">A unique sequence Id that can be used to store files with the same resourceId</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        /// <returns>A resource Id that can uniquely identify the file</returns>
        Task<Guid> StoreFileAsync(byte[] data, int? sequenceId, string? extension = null);

        /// <summary>
        /// Store a physical file in the repository
        /// </summary>
        /// <param name="data">The file data to store</param>
        /// <param name="resourceId">Specify the resource Id to use to save the file as</param>
        /// <param name="sequenceId">A unique sequence Id that can be used to store files with the same resourceId</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        /// <returns>A resource Id that can uniquely identify the file</returns>
        Task StoreFileAsync(byte[] data, Guid resourceId, int? sequenceId, string? extension = null);

        /// <summary>
        /// Store a physical file in the repository
        /// </summary>
        /// <param name="data">The file data to store</param>
        /// <param name="resourceId">Specify the resource Id to use to save the file as</param>
        /// <param name="filename">The unique filename of the file</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        /// <returns>A resource Id that can uniquely identify the file</returns>
        Task StoreFileAsync(byte[] data, Guid resourceId, string filename, string? extension = null);

        /// <summary>
        /// Store a physical file in the repository
        /// </summary>
        /// <param name="data">The file data to store</param>
        /// <param name="filename">The unique filename of the file</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        /// <returns>A resource Id that can uniquely identify the file</returns>
        Task StoreFileAsync(byte[] data, string filename, string? extension = null);

        /// <summary>
        /// Get a file by it's resource Id
        /// </summary>
        /// <param name="resourceId">A unique Id that identifies the file resource</param>
        /// <param name="sequenceId">A unique sequence Id that can be used to store files with the same resourceId</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        /// <returns></returns>
        Task<byte[]> GetFileAsync(Guid resourceId, int? sequenceId, string? extension = null);

        /// <summary>
        /// Get a file by it's resource Id and filename
        /// </summary>
        /// <param name="resourceId">A unique Id that identifies the file resource</param>
        /// <param name="filename">The unique filename of the file</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        /// <returns></returns>
        Task<byte[]> GetFileAsync(Guid resourceId, string filename, string? extension = null);

        /// <summary>
        /// Get a file by it's filename
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The unique filename of the file</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        /// <returns></returns>
        Task<byte[]> GetFileAsync(string filename, string? extension = null);

        /// <summary>
        /// Delete a file by it's resource Id
        /// </summary>
        /// <param name="resourceId">A unique Id that identifies the file resource</param>
        /// <param name="sequenceId">A unique sequence Id that can be used to store files with the same resourceId</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        Task DeleteFileAsync(Guid resourceId, int? sequenceId, string? extension = null);

        /// <summary>
        /// Delete a file by it's filename
        /// </summary>
        /// <param name="filename">Filename of file</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        /// <returns></returns>
        Task DeleteFileAsync(string filename, string? extension = null);

        /// <summary>
        /// Delete a file by it's resource Id and filename
        /// </summary>
        /// <param name="resourceId">A unique Id that identifies the file resource</param>
        /// <param name="filename">Filename of file</param>
        /// <param name="extension">Optionally include an extension for the file</param>
        /// <returns></returns>
        Task DeleteFileAsync(Guid resourceId, string filename, string? extension = null);

    }
}
