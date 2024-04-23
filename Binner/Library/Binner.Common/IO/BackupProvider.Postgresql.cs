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
        private async Task<MemoryStream> BackupPostgresqlAsync()
        {
            // backup: pg_dump database_name > filename.sql

            var toolPath = _configuration.ProviderConfiguration.ContainsKey("pg_dump") ? _configuration.ProviderConfiguration["pg_dump"] : LocatePgDump();
            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                ConnectionString = _configuration.ProviderConfiguration["ConnectionString"]
            };

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
                var output = await process.StandardOutput.ReadToEndAsync();
                var errors = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                var exitCode = process.ExitCode;
                if (!string.IsNullOrEmpty(errors) || string.IsNullOrEmpty(output))
                    throw new Exception($"Failed to export Postgresql database: {errors}");

                dbBytes = Encoding.UTF8.GetBytes(output);
            }

            return await BackupProviderAsync(dbBytes);
        }

        private async Task RestorePostgresqlAsync(DbInfo dbInfo)
        {
            // restore: psql database_name < filename.sql

            var filename = GetDatabaseBackupFilename();
            if (!Path.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);

            // write the database restore file to the file system
            await using var fs = File.Create(filename);
            dbInfo!.Database!.Position = 0;
            await dbInfo.Database.CopyToAsync(fs);
            fs.Close();

            var toolPath = _configuration.ProviderConfiguration.ContainsKey("psql") ? _configuration.ProviderConfiguration["psql"] : LocatePsql();
            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                ConnectionString = _configuration.ProviderConfiguration["ConnectionString"]
            };

            // drop the database if it exists
            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmdDrop = conn.CreateCommand();
            cmdDrop.CommandText = $"DROP DATABASE IF EXISTS {builder.Database} WITH (FORCE);";
            await cmdDrop.ExecuteNonQueryAsync();

            await using var cmdDrop2 = conn.CreateCommand();
            cmdDrop2.CommandText = $"DROP SCHEMA IF EXISTS dbo CASCADE;";
            await cmdDrop2.ExecuteNonQueryAsync();

            await using var cmdCreate = conn.CreateCommand();
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
                var output = await process.StandardOutput.ReadToEndAsync();
                var errors = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                var exitCode = process.ExitCode;
                if (!string.IsNullOrEmpty(errors) || string.IsNullOrEmpty(output))
                    throw new Exception($"Failed to restore Postgresql database: {errors}");
            }

            File.Delete(filename);
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
    }
}
