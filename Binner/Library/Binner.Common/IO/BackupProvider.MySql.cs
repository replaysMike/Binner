using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public partial class BackupProvider
    {
        private async Task<MemoryStream> BackupMySqlAsync()
        {
            // backup: mysqldump -u root -p sakila > C:\MySQLBackup\sakila_20200424.sql
            // restore: mysql -u root -p sakila < C:\MySQLBackup\sakila_20200424.sql

            var filename = GetDatabaseBackupFilename();
            if (!Path.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);
            var toolPath = _configuration.ProviderConfiguration.ContainsKey("mysqldump") ? _configuration.ProviderConfiguration["mysqldump"] : LocateMySqlDump();
            var builder = new MySqlConnector.MySqlConnectionStringBuilder
            {
                ConnectionString = _configuration.ProviderConfiguration["ConnectionString"]
            };

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
                var output = await process.StandardOutput.ReadToEndAsync();
                var errors = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                var exitCode = process.ExitCode;
                if (!string.IsNullOrEmpty(errors) || string.IsNullOrEmpty(output))
                    throw new Exception($"Failed to export MySql database: {errors}");

                dbBytes = Encoding.UTF8.GetBytes(output);
            }

            return await BackupProviderAsync(dbBytes);
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
            var builder = new MySqlConnector.MySqlConnectionStringBuilder
            {
                ConnectionString = _configuration.ProviderConfiguration["ConnectionString"]
            };

            // drop the database if it exists
            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var conn = context.Database.GetDbConnection();
            await using var dropCmd = conn.CreateCommand();
            await conn.OpenAsync();
            dropCmd.CommandText = $"DROP DATABASE IF EXISTS {builder.Database};";
            await dropCmd.ExecuteNonQueryAsync();

            await using var createCmd = conn.CreateCommand();
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
    }
}
