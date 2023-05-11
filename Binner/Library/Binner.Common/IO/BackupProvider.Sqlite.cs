using System.IO;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public partial class BackupProvider
    {
        private async Task<MemoryStream> BackupSqliteAsync()
        {
            var filename = _configuration.ProviderConfiguration["Filename"] ?? throw new BinnerConfigurationException("Error: no Filename specified in StorageProviderConfiguration");
            byte[] dbBytes;
            await using (var fileRef = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                dbBytes = new byte[fileRef.Length];
                fileRef.Read(dbBytes, 0, dbBytes.Length);
            }

            return await BackupProviderAsync(dbBytes);
        }

        private async Task RestoreSqliteAsync(DbInfo dbInfo)
        {
            var filename = _configuration.ProviderConfiguration["Filename"] ?? throw new BinnerConfigurationException("Error: no Filename specified in StorageProviderConfiguration");
            await using var fileRef = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            dbInfo!.Database!.Position = 0;
            await dbInfo.Database.CopyToAsync(fileRef);
        }
    }
}
