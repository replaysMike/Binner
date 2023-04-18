using System.IO;
using System.Threading.Tasks;

namespace Binner.Common.IO;

public interface IBackupProvider
{
    /// <summary>
    /// Backup the Binner installation
    /// </summary>
    /// <returns></returns>
    Task<MemoryStream> BackupAsync();

    /// <summary>
    /// Restore the Binner installation
    /// </summary>
    /// <param name="backupFile"></param>
    /// <returns></returns>
    Task RestoreAsync(UploadFile backupFile);
}