using Binner.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Binner.Services.IO
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

            // get a connection so we can use the handle to forcefully close the Sqlite database
            await using var context = await _contextFactory.CreateDbContextAsync();
            var conn = context.Database.GetDbConnection() as SqliteConnection;
            conn.Open();
            // forcefully close the sqlite database using it's handle
            var result = SQLitePCL.raw.sqlite3_close_v2(conn.Handle);
            conn.Handle.Close();
            conn.Handle.Dispose();

            // required GC collection
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to garbage collect while closing existing Sqlite database at '{filename}' for restore operation.");
            }

            var deleteSuccess = false;
            var attempts = 1;
            while ((attempts < 10) && (!deleteSuccess))
            {
                try
                {
                    Thread.Sleep(attempts * 100);
                    File.Delete(filename);
                    _logger.LogInformation($"Deleted existing Sqlite database at '{filename}' for restore operation.");
                    deleteSuccess = true;
                }
                catch (IOException e)   // delete only throws this on locking
                {
                    _logger.LogError($"Failed to delete existing Sqlite database at '{filename}' for restore operation. Retrying {attempts} of 10...");
                    attempts++;
                }
            }

            if (deleteSuccess)
            {
                await using var fileRef = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None);
                //dbInfo!.Database!.Position = 0;
                dbInfo.Database.Seek(0, SeekOrigin.Begin);
                await dbInfo.Database.CopyToAsync(fileRef);
                _logger.LogInformation($"Restored Sqlite database at '{filename}'.");
            }
            else
            {
                _logger.LogError($"Failed to overwrite existing Sqlite database at '{filename}' for restore operation.");
                throw new InvalidOperationException($"Unable to overwrite the current Sqlite database '{filename}'!");
            }
        }
    }
}
