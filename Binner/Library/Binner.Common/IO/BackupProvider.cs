using Binner.Data;
using Binner.Model.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public class BackupProvider : IBackupProvider
    {
        private readonly ILogger<BackupProvider> _logger;
        private readonly StorageProviderConfiguration _configuration;
        private readonly IDbContextFactory<BinnerContext> _contextFactory;

        public BackupProvider(ILogger<BackupProvider> logger, StorageProviderConfiguration configuration, IDbContextFactory<BinnerContext> contextFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _contextFactory = contextFactory;
        }

        public async Task<MemoryStream> BackupAsync()
        {
            try
            {
                var context = await _contextFactory.CreateDbContextAsync();
                switch (_configuration.Provider.ToLower())
                {
                    case "sqlite":
                    case "binner":
                        return await BackupSqliteAsync();
                    case "sqlserver":
                        return await BackupSqlServerAsync();
                    case "postgresql":
                        return await BackupPostgresqlAsync();
                    case "mysql":
                        return await BackupMySqlAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create a backup!");
                throw;
            }

            throw new NotSupportedException();
        }

        public async Task RestoreAsync(UploadFile backupFile)
        {
            try
            {
                var dbInfo = await RestoreFromBackupAsync(backupFile);
                if (dbInfo.BackupInfo == null) throw new Exception("Error: No backup information was found in the backup file.");
                if (dbInfo.Database == null) throw new Exception("Error: No database was found in the backup file.");
                if (dbInfo.Database.Length == 0) throw new Exception("Error: Refusing to restore from backup as the database length is 0.");
                if (!_configuration.Provider.Equals(dbInfo.BackupInfo.Provider, StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception($"You cannot restore a backup from provider '{dbInfo.BackupInfo.Provider}' onto the currently configured provider '{_configuration.Provider}' as this is not supported.");

                switch (_configuration.Provider.ToLower())
                {
                    case "sqlite":
                    case "binner":
                        await RestoreSqliteAsync(dbInfo);
                        await ProcessFileOperationsAsync(dbInfo);
                        return;
                    case "sqlserver":
                        await RestoreSqlServerAsync(dbInfo);
                        await ProcessFileOperationsAsync(dbInfo);
                        return;
                    case "postgresql":
                        await RestorePostgresqlAsync(dbInfo);
                        await ProcessFileOperationsAsync(dbInfo);
                        return;
                    case "mysql":
                        await RestoreMySqlAsync(dbInfo);
                        await ProcessFileOperationsAsync(dbInfo);
                        return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to restore from backup!");
                throw;
            }

            throw new NotSupportedException();
        }

        private async Task<MemoryStream> BackupSqliteAsync()
        {
            var filename = _configuration.ProviderConfiguration["Filename"] ?? throw new BinnerConfigurationException("Error: no Filename specified in StorageProviderConfiguration");
            byte[] dbBytes;
            using (var fileRef = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                dbBytes = new byte[fileRef.Length];
                fileRef.Read(dbBytes, 0, dbBytes.Length);
            }

            return await BackupProviderAsync(dbBytes);
        }

        private async Task<MemoryStream> BackupSqlServerAsync()
        {
            var filename = GetDatabaseBackupFilename();
            if (!Path.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder();
            builder.ConnectionString = _configuration.ProviderConfiguration["ConnectionString"];
            var dbName = builder["Database"];

            var context = await _contextFactory.CreateDbContextAsync();
            using var conn = context.Database.GetDbConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"BACKUP DATABASE [{dbName}] TO DISK = '{filename}' WITH FORMAT";
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();

            byte[] dbBytes;
            using (var fileRef = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                dbBytes = new byte[fileRef.Length];
                fileRef.Read(dbBytes, 0, dbBytes.Length);
            }

            File.Delete(filename);

            return await BackupProviderAsync(dbBytes);
        }

        private async Task<MemoryStream> BackupPostgresqlAsync()
        {
            // backup: pg_dump database_name > filename.sql

            var toolPath = _configuration.ProviderConfiguration.ContainsKey("pg_dump") ? _configuration.ProviderConfiguration["pg_dump"] : LocatePgDump();
            var builder = new Npgsql.NpgsqlConnectionStringBuilder();
            builder.ConnectionString = _configuration.ProviderConfiguration["ConnectionString"];

            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = $"\"host={builder.Host} port={builder.Port} dbname={builder.Database} user={builder.Username} password={builder.Password}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var dbBytes = Array.Empty<byte>();
            if (process.Start())
            {
                var output = process.StandardOutput.ReadToEnd();
                var errors = process.StandardError.ReadToEnd();
                await process.WaitForExitAsync();
                var exitCode = process.ExitCode;
                if (!string.IsNullOrEmpty(errors) || string.IsNullOrEmpty(output))
                    throw new Exception($"Failed to export Postgresql database: {errors}");

                dbBytes = Encoding.UTF8.GetBytes(output);
            }

            return await BackupProviderAsync(dbBytes);
        }

        private async Task<MemoryStream> BackupMySqlAsync()
        {
            // backup: mysqldump -u root -p sakila > C:\MySQLBackup\sakila_20200424.sql
            // restore: mysql -u root -p sakila < C:\MySQLBackup\sakila_20200424.sql

            var filename = GetDatabaseBackupFilename();
            if (!Path.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);
            var toolPath = _configuration.ProviderConfiguration.ContainsKey("mysqldump") ? _configuration.ProviderConfiguration["mysqldump"] : LocateMySqlDump();
            var builder = new MySqlConnector.MySqlConnectionStringBuilder();
            builder.ConnectionString = _configuration.ProviderConfiguration["ConnectionString"];

            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = $"-u {builder.UserID} -p\"{builder.Password}\" {builder.Database}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var dbBytes = Array.Empty<byte>();
            if (process.Start())
            {
                var output = process.StandardOutput.ReadToEnd();
                var errors = process.StandardError.ReadToEnd();
                await process.WaitForExitAsync();
                var exitCode = process.ExitCode;
                if (!string.IsNullOrEmpty(errors) || string.IsNullOrEmpty(output))
                    throw new Exception($"Failed to export MySql database: {errors}");

                dbBytes = Encoding.UTF8.GetBytes(output);
            }

            return await BackupProviderAsync(dbBytes);
        }

        private async Task<MemoryStream> BackupProviderAsync(byte[] dbBytes)
        {
            using var dbStream = new MemoryStream(dbBytes);
            var zipStream = new MemoryStream();
            using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // write the database
                var dbFileEntry = zipFile.CreateEntry("Binner.bak");
                using (var dbFileStream = dbFileEntry.Open())
                {
                    dbFileStream.Write(dbBytes, 0, dbBytes.Length);
                }

                // write the appsettings.json
                var settingsFilename = "appsettings.json";
                var configPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName) ?? string.Empty;
                var configFile = Path.Combine(configPath, settingsFilename);
                var settingsBytes = await File.ReadAllBytesAsync(configFile);
                var settingsFileEntry = zipFile.CreateEntry(settingsFilename);
                using (var settingsFileStream = settingsFileEntry.Open())
                {
                    settingsFileStream.Write(settingsBytes, 0, settingsBytes.Length);
                }

                // add all files in the UserFiles
                var userFiles = Directory.GetFiles(_configuration.UserUploadedFilesPath, "*", SearchOption.AllDirectories);
                var rootUri = new Uri(_configuration.UserUploadedFilesPath);
                foreach (var userFile in userFiles)
                {
                    var userFileUri = new Uri(Path.GetDirectoryName(userFile));
                    var userFilePath = rootUri.MakeRelativeUri(userFileUri);
                    var userFileBytes = await File.ReadAllBytesAsync(userFile);
                    var file = zipFile.CreateEntry(Path.Combine(userFilePath.OriginalString, Path.GetFileName(userFile)));
                    using (var fileStream = file.Open())
                    {
                        fileStream.Write(userFileBytes, 0, userFileBytes.Length);
                    }
                }

                // write a version file
                var buildVersion = Assembly.GetEntryAssembly()!.GetName()!.Version ?? new Version();
                var versionFile = zipFile.CreateEntry("Version");
                using (var versionFileStream = versionFile.Open())
                {
                    using var versionWriter = new BinaryWriter(versionFileStream);
                    var json = JsonConvert.SerializeObject(new BackupInfo
                    {
                        BackupDate = DateTime.UtcNow,
                        Provider = _configuration.Provider,
                        Version = buildVersion.ToString()
                    });
                    versionWriter.Write(json);
                }
            }
            zipStream.Seek(0, SeekOrigin.Begin);
            return zipStream;
        }

        private async Task<DbInfo> RestoreFromBackupAsync(UploadFile backupFile)
        {
            // extract the backup
            BackupInfo? backupInfo = null;
            MemoryStream? database = null;
            var fileOperations = new List<FileOperation>();
            using (var zipFile = new ZipArchive(backupFile.Stream, ZipArchiveMode.Read, false))
            {
                foreach (var file in zipFile.Entries)
                {
                    switch (file.Name)
                    {
                        case "appsettings.json":
                            {
                                var settingsFilename = "appsettings.json";
                                var configPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName) ?? string.Empty;
                                var configFile = Path.Combine(configPath, settingsFilename);
                                using var zipEntryStream = file.Open();
                                using var ms = new MemoryStream();
                                await zipEntryStream.CopyToAsync(ms);
                                fileOperations.Add(new FileOperation(configFile, ms.ToArray()));
                                break;
                            }
                        case "Version":
                            {
                                using var ms = new MemoryStream();
                                using var zipEntryStream = file.Open();
                                await zipEntryStream.CopyToAsync(ms);
                                ms.Position = 0;
                                var json = Encoding.UTF8.GetString(ms.ToArray());
                                if (!json.StartsWith("{"))
                                    json = json.Substring(1); // todo: remove
                                backupInfo = JsonConvert.DeserializeObject<BackupInfo>(json);
                                break;
                            }
                        case "Binner.bak":
                            {
                                database = new MemoryStream();
                                using var zipEntryStream = file.Open();
                                await zipEntryStream.CopyToAsync(database);
                                break;
                            }
                        default:
                            {
                                // user files
                                var basePath = new Uri(_configuration.UserUploadedFilesPath);
                                var relativePath = new Uri(file.FullName.Replace("UserFiles/", ""), UriKind.Relative);
                                var destinationPath = Path.Combine(basePath.OriginalString, relativePath.OriginalString);
                                using var zipEntryStream = file.Open();
                                using var ms = new MemoryStream();
                                await zipEntryStream.CopyToAsync(ms);
                                fileOperations.Add(new FileOperation(destinationPath, ms.ToArray()));
                                break;
                            }
                    }
                }
            }
            return new DbInfo
            {
                BackupInfo = backupInfo,
                Database = database,
                FileOperations = fileOperations
            };
        }

        private async Task ProcessFileOperationsAsync(DbInfo dbInfo)
        {
            foreach (var file in dbInfo.FileOperations)
            {
                switch (file.Operation)
                {
                    case "Create":
                        var directory = Path.GetDirectoryName(file.DestinationFilename);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                            Directory.CreateDirectory(directory);
                        await File.WriteAllBytesAsync(file.DestinationFilename, file.Data);
                        break;
                }
            }
        }

        private async Task RestoreSqliteAsync(DbInfo dbInfo)
        {
            var filename = _configuration.ProviderConfiguration["Filename"] ?? throw new BinnerConfigurationException("Error: no Filename specified in StorageProviderConfiguration");
            using (var fileRef = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                dbInfo!.Database!.Position = 0;
                await dbInfo.Database.CopyToAsync(fileRef);
            }
        }

        private async Task RestoreSqlServerAsync(DbInfo dbInfo)
        {
            var filename = GetDatabaseBackupFilename();
            if (!Path.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);

            using var fileRef = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            dbInfo!.Database!.Position = 0;
            await dbInfo.Database.CopyToAsync(fileRef);
            fileRef.Close();

            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder();
            builder.ConnectionString = _configuration.ProviderConfiguration["ConnectionString"];
            var dbName = builder["Database"];

            var context = await _contextFactory.CreateDbContextAsync();
            using var conn = context.Database.GetDbConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"USE [master]; 
ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [{dbName}] FROM DISK = '{filename}';
ALTER DATABASE [{dbName}] SET MULTI_USER;
";
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        private async Task RestorePostgresqlAsync(DbInfo dbInfo)
        {
            // restore: psql database_name < filename.sql

            var filename = GetDatabaseBackupFilename();
            if (!Path.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);

            // write the database restore file to the file system
            using var fs = File.Create(filename);
            dbInfo!.Database!.Position = 0;
            await dbInfo.Database.CopyToAsync(fs);
            fs.Close();

            var toolPath = _configuration.ProviderConfiguration.ContainsKey("psql") ? _configuration.ProviderConfiguration["psql"] : LocatePsql();
            var builder = new Npgsql.NpgsqlConnectionStringBuilder();
            builder.ConnectionString = _configuration.ProviderConfiguration["ConnectionString"];

            // drop the database if it exists
            var context = await _contextFactory.CreateDbContextAsync();
            using var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmdDrop = conn.CreateCommand();
            cmdDrop.CommandText = $"DROP DATABASE IF EXISTS {builder.Database} WITH (FORCE);";
            await cmdDrop.ExecuteNonQueryAsync();
            
            using var cmdDrop2 = conn.CreateCommand();
            cmdDrop2.CommandText = $"DROP SCHEMA IF EXISTS dbo CASCADE;";
            await cmdDrop2.ExecuteNonQueryAsync();

            using var cmdCreate = conn.CreateCommand();
            cmdCreate.CommandText = $"CREATE DATABASE {builder.Database};";
            await cmdCreate.ExecuteNonQueryAsync();

            await conn.CloseAsync();

            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = $"-f \"{filename}\" \"host={builder.Host} port={builder.Port} dbname={builder.Database} user={builder.Username} password={builder.Password}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var dbBytes = Array.Empty<byte>();
            if (process.Start())
            {
                var output = process.StandardOutput.ReadToEnd();
                var errors = process.StandardError.ReadToEnd();
                await process.WaitForExitAsync();
                var exitCode = process.ExitCode;
                if (!string.IsNullOrEmpty(errors) || string.IsNullOrEmpty(output))
                    throw new Exception($"Failed to restore Postgresql database: {errors}");
            }

            File.Delete(filename);
        }

        private async Task RestoreMySqlAsync(DbInfo dbInfo)
        {
            // restore: mysql < filename.sql
            var filename = GetDatabaseBackupFilename();
            if (!Path.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);


            // mysql doesn't support specifying the filename - relying on piping - so we don't need the filesystem
            var script = Encoding.UTF8.GetString(dbInfo.Database.ToArray());

            var toolPath = _configuration.ProviderConfiguration.ContainsKey("mysql") ? _configuration.ProviderConfiguration["mysql"] : LocateMySql();
            var builder = new MySqlConnector.MySqlConnectionStringBuilder();
            builder.ConnectionString = _configuration.ProviderConfiguration["ConnectionString"];

            // drop the database if it exists
            var context = await _contextFactory.CreateDbContextAsync();
            using var conn = context.Database.GetDbConnection();
            using var dropCmd = conn.CreateCommand();
            await conn.OpenAsync();
            dropCmd.CommandText = $"DROP DATABASE IF EXISTS {builder.Database};";
            await dropCmd.ExecuteNonQueryAsync();

            using var createCmd = conn.CreateCommand();
            createCmd.CommandText = $"CREATE DATABASE {builder.Database};";
            await createCmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();

            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = $"-u {builder.UserID} -p\"{builder.Password}\" {builder.Database}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };
            var dbBytes = Array.Empty<byte>();
            if (process.Start())
            {
                // pipe the script into mysql tool
                await process.StandardInput.WriteAsync(script);
                await process.StandardInput.FlushAsync();
                process.StandardInput.Close();

                // get response (no output and no error is normal success)
                var output = await process.StandardOutput.ReadToEndAsync();
                var errors = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                var exitCode = process.ExitCode;
                if (!string.IsNullOrEmpty(errors))
                    throw new Exception($"Failed to restore MySql database: {errors}");
            }
        }

        private string LocatePgDump()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                for (var i = 25; i >= 11; i--)
                {
                    var path = $@"C:\Program Files\PostgreSQL\{i}\bin\pg_dump.exe";
                    if (File.Exists(path)) return path;
                }
            }
            else
            {
                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "find",
                    Arguments = "/ -name pg_dump -type f 2>/dev/null",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process.Start();
                process.WaitForExit();
                var output = process.StandardOutput.ReadToEnd();
                var lines = output?.Split('\n');
                if (lines?.Length > 0)
                    return lines[0];
            }

            return "pg_dump";
        }

        private string LocatePsql()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                for (var i = 25; i >= 11; i--)
                {
                    var path = $@"C:\Program Files\PostgreSQL\{i}\bin\psql.exe";
                    if (File.Exists(path)) return path;
                }
            }
            else
            {
                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "find",
                    Arguments = "/ -name psql -type f 2>/dev/null",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process.Start();
                process.WaitForExit();
                var output = process.StandardOutput.ReadToEnd();
                var lines = output?.Split('\n');
                if (lines?.Length > 0)
                    return lines[0];
            }

            return "psql";
        }

        private string LocateMySqlDump()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var folders = Directory.GetDirectories(@"C:\Program Files", "MariaDB*", SearchOption.TopDirectoryOnly);
                if (folders.Length == 0)
                    folders = Directory.GetDirectories(@"C:\Program Files", "MySql*", SearchOption.TopDirectoryOnly);
                if (folders.Length > 0)
                {
                    foreach (var folder in folders)
                    {
                        var toolPath = Path.Combine(folder, "./bin/mariadb-dump.exe");
                        if (File.Exists(toolPath)) return toolPath;
                        toolPath = Path.Combine(folder, "./bin/mysqldump.exe");
                        if (File.Exists(toolPath)) return toolPath;
                    }
                }
            }
            else
            {
                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "find",
                    Arguments = "/ -name mysqldump -type f 2>/dev/null",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                var lines = output?.Split('\n');
                if (lines?.Length > 0)
                    return lines[0];
            }

            return "mysqldump";
        }

        private string LocateMySql()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var folders = Directory.GetDirectories(@"C:\Program Files", "MariaDB*", SearchOption.TopDirectoryOnly);
                if (folders.Length == 0)
                    folders = Directory.GetDirectories(@"C:\Program Files", "MySql*", SearchOption.TopDirectoryOnly);
                if (folders.Length > 0)
                {
                    foreach (var folder in folders)
                    {
                        var toolPath = Path.Combine(folder, "./bin/mysql.exe");
                        if (File.Exists(toolPath)) return toolPath;
                        toolPath = Path.Combine(folder, "./bin/mysql.exe");
                        if (File.Exists(toolPath)) return toolPath;
                    }
                }
            }
            else
            {
                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "find",
                    Arguments = "/ -name mysql -type f 2>/dev/null",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                var lines = output?.Split('\n');
                if (lines?.Length > 0)
                    return lines[0];
            }

            return "mysql";
        }

        private string GetDatabaseBackupFilename()
        {
            var filename = _configuration.ProviderConfiguration.ContainsKey("Filename") ? _configuration.ProviderConfiguration["Filename"] : "";
            if (string.IsNullOrEmpty(filename))
            {
                var appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName) ?? string.Empty;
                filename = Path.Combine(appPath, Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            }

            filename = Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, "Backups", Path.GetFileNameWithoutExtension(filename)) + ".bak";
            return filename;
        }
    }

    public class BackupInfo
    {
        public DateTime BackupDate { get; set; }
        public string Provider { get; set; } = null!;
        public string Version { get; set; } = null!;
    }

    public class DbInfo
    {
        public BackupInfo? BackupInfo { get; set; }
        public MemoryStream? Database { get; set; }
        public List<FileOperation> FileOperations { get; set; } = new List<FileOperation>();
    }

    public class FileOperation
    {
        public string Operation { get; set; } = "Create";
        public string DestinationFilename { get; set; }
        public byte[] Data { get; set; }

        public FileOperation(string destinationFilename, byte[] data)
        {
            DestinationFilename = destinationFilename;
            Data = data;
        }
    }
}
