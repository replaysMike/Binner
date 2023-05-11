using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public partial class BackupProvider
    {
        private async Task<MemoryStream> BackupSqlServerAsync()
        {
            var filename = GetDatabaseBackupFilename();
            if (!Path.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
            {
                ConnectionString = _configuration.ProviderConfiguration["ConnectionString"]
            };
            var dbName = builder["Database"];

            var context = await _contextFactory.CreateDbContextAsync();
            await using var conn = context.Database.GetDbConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"BACKUP DATABASE [{dbName}] TO DISK = '{filename}' WITH FORMAT";
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();

            byte[] dbBytes;
            await using (var fileRef = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                dbBytes = new byte[fileRef.Length];
                fileRef.Read(dbBytes, 0, dbBytes.Length);
            }

            File.Delete(filename);

            return await BackupProviderAsync(dbBytes);
        }

        private async Task RestoreSqlServerAsync(DbInfo dbInfo)
        {
            var filename = GetDatabaseBackupFilename();
            if (!Path.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);

            await using var fileRef = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            dbInfo!.Database!.Position = 0;
            await dbInfo.Database.CopyToAsync(fileRef);
            fileRef.Close();

            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
            {
                ConnectionString = _configuration.ProviderConfiguration["ConnectionString"]
            };
            var dbName = builder["Database"];

            var context = await _contextFactory.CreateDbContextAsync();
            await using var conn = context.Database.GetDbConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"USE [master]; 
ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [{dbName}] FROM DISK = '{filename}';
ALTER DATABASE [{dbName}] SET MULTI_USER;
";
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
