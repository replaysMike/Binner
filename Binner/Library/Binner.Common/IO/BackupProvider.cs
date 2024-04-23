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
    public partial class BackupProvider : IBackupProvider
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
                        try
                        {
                            await RestoreSqliteAsync(dbInfo);
                        }
                        catch (ObjectDisposedException)
                        {
                            // expected, unavoidable
                        }
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

        private async Task<MemoryStream> BackupProviderAsync(byte[] dbBytes)
        {
            using var dbStream = new MemoryStream(dbBytes);
            var zipStream = new MemoryStream();
            using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // write the database
                var dbFileEntry = zipFile.CreateEntry("Binner.bak");
                await using (var dbFileStream = dbFileEntry.Open())
                {
                    await dbFileStream.WriteAsync(dbBytes, 0, dbBytes.Length);
                }

                // write the appsettings.json
                var settingsFilename = "appsettings.json";
                var configPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName) ?? string.Empty;
                var configFile = Path.Combine(configPath, settingsFilename);
                var settingsBytes = await File.ReadAllBytesAsync(configFile);
                var settingsFileEntry = zipFile.CreateEntry(settingsFilename);
                await using (var settingsFileStream = settingsFileEntry.Open())
                {
                    await settingsFileStream.WriteAsync(settingsBytes, 0, settingsBytes.Length);
                }

                // add all files in the UserFiles
                if (Directory.Exists(_configuration.UserUploadedFilesPath))
                {
                    var userFiles = Directory.GetFiles(_configuration.UserUploadedFilesPath, "*", SearchOption.AllDirectories);
                    var fullPath = Path.GetFullPath(_configuration.UserUploadedFilesPath);
                    var rootUri = new Uri(fullPath);
                    foreach (var userFile in userFiles)
                    {
                        var directoryName = Path.GetDirectoryName(userFile);
                        var userFileUri = new Uri(directoryName);
                        var userFilePath = rootUri.MakeRelativeUri(userFileUri);
                        var userFileBytes = await File.ReadAllBytesAsync(userFile);
                        var file = zipFile.CreateEntry(Path.Combine(userFilePath.OriginalString, Path.GetFileName(userFile)));
                        await using var fileStream = file.Open();
                        await fileStream.WriteAsync(userFileBytes, 0, userFileBytes.Length);
                    }
                }

                // write a version file
                var buildVersion = Assembly.GetEntryAssembly()!.GetName()!.Version ?? new Version();
                var versionFile = zipFile.CreateEntry("Version");
                await using (var versionFileStream = versionFile.Open())
                {
                    await using var versionWriter = new BinaryWriter(versionFileStream);
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
                                await using var zipEntryStream = file.Open();
                                using var ms = new MemoryStream();
                                await zipEntryStream.CopyToAsync(ms);
                                fileOperations.Add(new FileOperation(configFile, ms.ToArray()));
                                break;
                            }
                        case "Version":
                            {
                                using var ms = new MemoryStream();
                                await using var zipEntryStream = file.Open();
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
                                await using var zipEntryStream = file.Open();
                                await zipEntryStream.CopyToAsync(database);
                                break;
                            }
                        default:
                            {
                                // user files
                                var basePath = new Uri(_configuration.UserUploadedFilesPath);
                                var relativePath = new Uri(file.FullName.Replace("UserFiles/", ""), UriKind.Relative);
                                var destinationPath = Path.Combine(basePath.OriginalString, relativePath.OriginalString);
                                await using var zipEntryStream = file.Open();
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
        public List<FileOperation> FileOperations { get; set; } = new();
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

        public override string ToString() => DestinationFilename;
    }
}
