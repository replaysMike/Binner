using AnySerializer;
using Binner.Data;
using Binner.Global.Common;
using Binner.Legacy;
using Binner.Legacy.StorageProviders;
using Binner.Model;
using Binner.Model.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Binner.Web.Database
{
    public class MigrationHandler
    {
        private readonly StorageProviderConfiguration _config;
        private readonly Logger _logger;
        private readonly SerializerOptions _serializationOptions = SerializerOptions.EmbedTypes;
        private readonly SerializerProvider _serializer = new();
        private readonly IDbContextFactory<BinnerContext> _contextFactory;

        public MigrationHandler(Logger logger, StorageProviderConfiguration configuration, IDbContextFactory<BinnerContext> contextFactory)
        {
            _logger = logger;
            _config = configuration;
            _contextFactory = contextFactory;
        }

        public bool TryDetectMigrateNeeded(out Binner.Model.Common.IBinnerDb? binnerDb)
        {
            binnerDb = null;

            switch (_config.Provider.ToLower())
            {
                case "binner":
                    {
                        var fsConfig = new BinnerFileStorageConfiguration(_config.ProviderConfiguration);
                        // check if the database is a Binner File storage db, or a sqlite db
                        if (File.Exists(fsConfig.Filename))
                        {
                            try
                            {
                                using (var stream = File.Open(fsConfig.Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                                {
                                    var version = ReadDbVersion(stream);
                                    if (version == null)
                                    {
                                        _logger.Info("Binner file database is Sqlite format, no migration required.");
                                        return false;
                                    }

                                    if (version.Version >= 1 && version.Version <= 7)
                                    {
                                        // it's a valid db.
                                        try
                                        {
                                            // copy the rest of the bytes
                                            var bytes = new byte[stream.Length - stream.Position];
                                            stream.Read(bytes, 0, bytes.Length);
                                            var db = LoadDatabaseByVersion(version, bytes);
                                            if (ValidateChecksum(db))
                                            {
                                                binnerDb = db;
                                                _logger.Info("Migrating Binner file database to Sqlite...");
                                                // must migrate this database
                                                return true;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // something failed, not a valid Binner db
                                        }
                                    }
                                }
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                // file is locked, cannot continue.
                                _logger.Error(ex, "Database is locked by another process.");
                                throw;
                            }
                            catch (ArgumentException ex)
                            {
                                // not a binner db
                                _logger.Warn(ex, "Database not a Binner format database.");
                            }
                            catch (IOException ex)
                            {
                                // not a binner db
                                _logger.Warn(ex, "Database cannot be accessed.");
                            }
                            catch (Exception ex)
                            {
                                // invalid format or could not read
                                _logger.Warn(ex, "Refusing to migrate database, probably not a Binner formatted database file. An exception occurred.");
                            }

                        }

                        return false;
                    }
                case "mysql":
                    {
                        using var context = _contextFactory.CreateDbContext();

                        // if the database doesn't exist, we don't need to migrate
                        if (!context.Database.CanConnect())
                            return false;

                        using var conn = context.Database.GetDbConnection();

                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_NAME='__EFMigrationsHistory' AND TABLE_TYPE = 'BASE TABLE'";
                        conn.Open();
                        var exists = (long?)cmd.ExecuteScalar();
                        // if it doesn't exist, a migration is required
                        return !(exists > 0);
                    }
                case "postgresql":
                    {
                        using var context = _contextFactory.CreateDbContext();

                        // if the database doesn't exist, we don't need to migrate
                        if (!context.Database.CanConnect())
                            return false;

                        using var conn = context.Database.GetDbConnection();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM pg_tables WHERE schemaname='dbo' AND tablename = '__EFMigrationsHistory'";
                        conn.Open();
                        var exists = (long?)cmd.ExecuteScalar();
                        // if it doesn't exist, a migration is required
                        return !(exists > 0);
                    }
                case "sqlite":
                    {
                        using var context = _contextFactory.CreateDbContext();

                        // if the database doesn't exist, we don't need to migrate
                        if (!context.Database.CanConnect())
                            return false;

                        using var conn = context.Database.GetDbConnection();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM __EFMigrationsHistory";
                        conn.Open();
                        try
                        {
                            cmd.ExecuteScalar();
                            // no exception, table exists
                            return false;
                        }
                        catch (SqliteException)
                        {
                            // table does not exist
                            return true;
                        }
                    }
                case "sqlserver":
                    {
                        using var context = _contextFactory.CreateDbContext();

                        // if the database doesn't exist, we don't need to migrate
                        if (!context.Database.CanConnect())
                            return false;

                        using var conn = context.Database.GetDbConnection();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM sysobjects WHERE name='__EFMigrationsHistory' and xtype='U'";
                        conn.Open();
                        var exists = (int?)cmd.ExecuteScalar();
                        // if it doesn't exist, a migration is required
                        return !(exists > 0);
                    }
            }


            // no migration needed


            return false;
        }

        public bool MigrateDatabase(Binner.Model.Common.IBinnerDb binnerDb)
        {
            // all new users have an id=1 as no users existed prior to this migration
            var userId = 1;
#if INITIALCREATE
            var defaultUser = new Data.Model.User
            {
                Name = "Admin",
                EmailAddress = "admin",
                PasswordHash = PasswordHasher.GeneratePasswordHash("admin").GetBase64EncodedPasswordHash(),
                DateCreatedUtc = DateTime.UtcNow,
                DateModifiedUtc = DateTime.UtcNow,
                IsEmailConfirmed = true,
                IsAdmin = true,
                OrganizationId = 1
            };
#endif

            switch (_config.Provider.ToLower())
            {
                case "binner":
                    {
                        return MigrateBinnerToSqlite(binnerDb, userId);
                    }
                case "mysql":
                    {
                        var context = _contextFactory.CreateDbContext();
                        // is this a brand new database or pre-existing? Brand new databases need the InitialCreate migration to be applied
                        using var conn = context.Database.GetDbConnection(); 
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_NAME='OAuthCredentials' AND TABLE_TYPE = 'BASE TABLE'";
                        conn.Open();
                        var exists = ((long?)cmd.ExecuteScalar()) > 0;
                        conn.Close();

                        if (exists)
                        {
                            var migrationName = "20230412193145_InitialCreate";
                            var efVersion = "7.0.4";
                            context.Database.ExecuteSql(
                                $@"CREATE TABLE __EFMigrationsHistory(MigrationId nvarchar(150) NOT NULL primary key, ProductVersion nvarchar(32) NOT NULL)");
                            context.Database.ExecuteSql($"INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES({migrationName}, {efVersion});");
                        }

                        // run the next migration which will apply most recent schema
                        context.Database.Migrate();
#if INITIALCREATE
                        if (exists)
                        {
                            // seed the default user
                            context.Users.Add(defaultUser);
                        }
#endif
                        context.SaveChanges();
                        context.Database.ExecuteSql($"UPDATE OAuthCredentials SET UserId={userId};UPDATE OAuthRequests SET UserId={userId};UPDATE Parts SET UserId={userId};UPDATE PartSuppliers SET UserId={userId};UPDATE PartTypes SET UserId={userId};UPDATE Pcbs SET UserId={userId};UPDATE PcbStoredFileAssignments SET UserId={userId};UPDATE ProjectPartAssignments SET UserId={userId};UPDATE ProjectPcbAssignments SET UserId={userId};UPDATE Projects SET UserId={userId};UPDATE StoredFiles SET UserId={userId};");
                    }
                    break;
                case "postgresql":
                    {
                        try
                        {
                            var context = _contextFactory.CreateDbContext();

                            // is this a brand new database or pre-existing? Brand new databases need the InitialCreate migration to be applied
                            using var conn = context.Database.GetDbConnection(); 
                            using var cmd = conn.CreateCommand();
                            cmd.CommandText = "SELECT COUNT(*) FROM pg_tables WHERE schemaname='dbo' AND tablename='OAuthCredentials'";
                            conn.Open();
                            var exists = ((long?)cmd.ExecuteScalar()) > 0;
                            conn.Close();

                            if (exists)
                            {
                                var migrationName = "20230412193100_InitialCreate";
                                var efVersion = "7.0.4";
                                context.Database.ExecuteSql(
                                    $@"CREATE TABLE dbo.""__EFMigrationsHistory"" (""MigrationId"" varchar(150) NOT NULL PRIMARY KEY, ""ProductVersion"" varchar(32) NOT NULL);");
                                context.Database.ExecuteSql(
                                    $@"INSERT INTO dbo.""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") VALUES({migrationName}, {efVersion});");
                            }

                            // run the next migration which will apply most recent schema
                            context.Database.Migrate();
#if INITIALCREATE
                            if (exists)
                            {
                                // seed the default user
                                context.Users.Add(defaultUser);
                            }
#endif
                            context.SaveChanges();
                            context.Database.ExecuteSql(
                                $@"UPDATE dbo.""OAuthCredentials"" SET ""UserId""={userId};UPDATE dbo.""OAuthRequests"" SET ""UserId""={userId};UPDATE dbo.""Parts"" SET ""UserId""={userId};UPDATE dbo.""PartSuppliers"" SET ""UserId""={userId};UPDATE dbo.""PartTypes"" SET ""UserId""={userId};UPDATE dbo.""Pcbs"" SET ""UserId""={userId};UPDATE dbo.""PcbStoredFileAssignments"" SET ""UserId""={userId};UPDATE dbo.""ProjectPartAssignments"" SET ""UserId""={userId};UPDATE dbo.""ProjectPcbAssignments"" SET ""UserId""={userId};UPDATE dbo.""Projects"" SET ""UserId""={userId};UPDATE dbo.""StoredFiles"" SET ""UserId""={userId};");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Could not migrate Postgresql database!");
                            return false;
                        }
                    }
                    break;
                case "sqlite":
                    {
                        var context = _contextFactory.CreateDbContext();
                        // is this a brand new database or pre-existing? Brand new databases need the InitialCreate migration to be applied
                        using var conn = context.Database.GetDbConnection(); 
                        using var cmd = conn.CreateCommand();
                        
                        cmd.CommandText = "SELECT COUNT(*) FROM OAuthCredentials";
                        conn.Open();
                        var exists = false;
                        try
                        {
                            cmd.ExecuteScalar();
                            // no exception, table exists
                            exists = true;
                        }
                        catch (SqliteException)
                        {
                            // table does not exist
                            exists = false;
                        }
                        
                        if (exists)
                        {
                            var migrationName = "20230412171856_InitialCreate";
                            var efVersion = "7.0.4";
                            context.Database.ExecuteSql($@"CREATE TABLE __EFMigrationsHistory(MigrationId TEXT NOT NULL PRIMARY KEY, ProductVersion TEXT NOT NULL)");
                            context.Database.ExecuteSql($"INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES({migrationName}, {efVersion})");
                        }

                        // run the next migration which will apply most recent schema
                        context.Database.Migrate();
#if INITIALCREATE
                        if (exists)
                        {
                            // seed the default user
                            context.Users.Add(defaultUser);
                        }
#endif
                        context.SaveChanges();
                        context.Database.ExecuteSql($"UPDATE OAuthCredentials SET UserId={userId};UPDATE OAuthRequests SET UserId={userId};UPDATE Parts SET UserId={userId};UPDATE PartSuppliers SET UserId={userId};UPDATE PartTypes SET UserId={userId};UPDATE Pcbs SET UserId={userId};UPDATE PcbStoredFileAssignments SET UserId={userId};UPDATE ProjectPartAssignments SET UserId={userId};UPDATE ProjectPcbAssignments SET UserId={userId};UPDATE Projects SET UserId={userId};UPDATE StoredFiles SET UserId={userId};");
                    }
                    break;
                case "sqlserver":
                    {
                        // create a __EFMigrationsHistory table, and add the InitialCreate record
                        var context = _contextFactory.CreateDbContext();
                        // is this a brand new database or pre-existing? Brand new databases need the InitialCreate migration to be applied
                        using var conn = context.Database.GetDbConnection(); 
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM sysobjects WHERE name='OAuthCredentials' and xtype='U'";
                        conn.Open();
                        var exists = ((int?)cmd.ExecuteScalar()) > 0;
                        conn.Close();

                        if (exists)
                        {
                            var migrationName = "20230412193201_InitialCreate";
                            var efVersion = "7.0.4";
                            context.Database.ExecuteSql(
                                $@"IF NOT EXISTS (select * from sysobjects where name='__EFMigrationsHistory' and xtype='U') CREATE TABLE [dbo].[__EFMigrationsHistory]([MigrationId] [nvarchar](150) NOT NULL, [ProductVersion] [nvarchar](32) NOT NULL, CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]");
                            context.Database.ExecuteSql($"INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES({migrationName}, {efVersion});");
                        }

                        // run the next migration which will apply most recent schema
                        context.Database.Migrate();
#if INITIALCREATE
                        if (exists)
                        {
                            // seed the default user
                            context.Users.Add(defaultUser);
                        }
#endif
                        context.SaveChanges();
                        context.Database.ExecuteSql($"UPDATE OAuthCredentials SET UserId={userId};UPDATE OAuthRequests SET UserId={userId};UPDATE Parts SET UserId={userId};UPDATE PartSuppliers SET UserId={userId};UPDATE PartTypes SET UserId={userId};UPDATE Pcbs SET UserId={userId};UPDATE PcbStoredFileAssignments SET UserId={userId};UPDATE ProjectPartAssignments SET UserId={userId};UPDATE ProjectPcbAssignments SET UserId={userId};UPDATE Projects SET UserId={userId};UPDATE StoredFiles SET UserId={userId};");
                    }
                    break;
            }

            return true;
        }

        private bool MigrateBinnerToSqlite(Binner.Model.Common.IBinnerDb binnerDb, int userId)
        {
            // steps for migration:
            // 1) backup existing db
            // 2) 
            var fsConfig = new BinnerFileStorageConfiguration(_config.ProviderConfiguration);
            var backupFile = Path.Combine(Path.GetDirectoryName(fsConfig.Filename), $"{Path.GetFileName(fsConfig.Filename)}.BinnerFileStorageProvider.backup");
            if (File.Exists(fsConfig.Filename))
            {
                if (!File.Exists(backupFile))
                {
                    try
                    {
                        _logger.Info("Preparing to migrate Binner database to Sqlite...");

                        _logger.Info($"Backing up BinnerDbV7 database to '{backupFile}'");
                        // create backup
                        File.Copy(fsConfig.Filename, backupFile);

                        // import it's data into a new Entity Framework database, by default Sqlite.
                        var newConfiguration = new Binner.Model.Configuration.StorageProviderConfiguration
                        {
                            Provider = "Sqlite",
                            ProviderConfiguration = new Dictionary<string, string>
                            {
                                { "ConnectionString", $@"Data Source={fsConfig.Filename};" }
                            }
                        };

                        // remove the original database file (it's been backed up)
                        _logger.Info($"Deleting existing BinnerDbV7 database");
                        File.Delete(fsConfig.Filename);

                        using var context = _contextFactory.CreateDbContext();
                        _logger.Info($"Creating migrations table '__EFMigrationsHistory'");
                        //var migrationName = "20230412171856_InitialCreate";
                        //var efVersion = "7.0.4";
                        //context.Database.ExecuteSql($@"CREATE TABLE __EFMigrationsHistory(MigrationId TEXT NOT NULL PRIMARY KEY, ProductVersion TEXT NOT NULL);");
                        //context.Database.ExecuteSql($"INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES({migrationName}, {efVersion});");

                        // create the Sqlite database
                        _logger.Info("Migrating Binner database to Sqlite...");

                        context.Database.Migrate();

                        // import data
                        ImportBinnerDb(binnerDb, context, userId);

                        // reapply the data migration for Sqlite, because there was no data during migrate no OrganizationIds were set as intended.
                        context.Database.ExecuteSql($@"
UPDATE OAuthCredentials SET OrganizationId = 1;
UPDATE OAuthRequests SET OrganizationId = 1;
UPDATE Parts SET OrganizationId = 1;
UPDATE PartSuppliers SET OrganizationId = 1;
UPDATE PartTypes SET OrganizationId = 1;
UPDATE Pcbs SET OrganizationId = 1;
UPDATE PcbStoredFileAssignments SET OrganizationId = 1;
UPDATE ProjectPartAssignments SET OrganizationId = 1;
UPDATE ProjectPcbAssignments SET OrganizationId = 1;
UPDATE Projects SET OrganizationId = 1;
UPDATE StoredFiles SET OrganizationId = 1;
UPDATE UserIntegrationConfigurations SET OrganizationId = 1;
UPDATE UserLoginHistory SET OrganizationId = 1;
UPDATE UserPrinterConfigurations SET OrganizationId = 1;
UPDATE UserPrinterTemplateConfigurations SET OrganizationId = 1;
UPDATE Users SET OrganizationId = 1;
UPDATE UserTokens SET OrganizationId = 1;
UPDATE Users SET OrganizationId = 1;
");

                        _logger.Info("Successfully migrated Binner database to Sqlite!");
                        // success!
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // copy the backup back
                        File.Move(backupFile, fsConfig.Filename);
                        _logger.Error(ex, $"Failed to migrate database.");
                    }
                    finally
                    {
                        if (!File.Exists(fsConfig.Filename) && File.Exists(backupFile))
                            File.Move(backupFile, fsConfig.Filename);
                    }
                }
                else
                {
                    _logger.Error($"Refusing to migrate database to new format. Binner database file has already been backed up to '{backupFile}', indicating a possible previous failure. Delete or move this file to try again.");
                    return false;
                }
            }
            else
            {
                if (File.Exists(backupFile))
                    _logger.Error($"Refusing to migrate database. Binner database file does not exist at '{fsConfig.Filename}'. Rename or move the backup file '{backupFile}' to '{fsConfig.Filename}' to retry.");
                else
                    _logger.Error($"Refusing to migrate database. Binner database file does not exist at '{fsConfig.Filename}'.");
                return false;
            }
            return false;
        }

        private Binner.Model.Common.IBinnerDb LoadDatabaseByVersion(BinnerDbVersion version, byte[] bytes)
        {
            Binner.Model.Common.IBinnerDb db;
            // Support database loading by version number
            try
            {
                db = version.Version switch
                {
                    // Version 1 (upgrade required)
                    BinnerDbV1.VersionNumber => new BinnerDbV7(_serializer.Deserialize<BinnerDbV1>(bytes, _serializationOptions, new PropertyVersion("BinnerDbV4", "BinnerDbV5", "BinnerDbV6", "BinnerDbV7")), (upgradeDb) => BuildChecksum(upgradeDb)),
                    // Version 2 (upgrade required)
                    BinnerDbV2.VersionNumber => new BinnerDbV7(_serializer.Deserialize<BinnerDbV2>(bytes, _serializationOptions, new PropertyVersion("BinnerDbV4", "BinnerDbV5", "BinnerDbV6", "BinnerDbV7")), (upgradeDb) => BuildChecksum(upgradeDb)),
                    // Version 3 (upgrade required)
                    BinnerDbV3.VersionNumber => new BinnerDbV7(_serializer.Deserialize<BinnerDbV3>(bytes, _serializationOptions, new PropertyVersion("BinnerDbV4", "BinnerDbV5", "BinnerDbV6", "BinnerDbV7")), (upgradeDb) => BuildChecksum(upgradeDb)),
                    // Version 4 (upgrade required)
                    BinnerDbV4.VersionNumber => new BinnerDbV7(_serializer.Deserialize<BinnerDbV4>(bytes, _serializationOptions, new PropertyVersion("BinnerDbV5", "BinnerDbV6", "BinnerDbV7")), (upgradeDb) => BuildChecksum(upgradeDb)),
                    // Version 5 (upgrade required)
                    BinnerDbV5.VersionNumber => new BinnerDbV7(_serializer.Deserialize<BinnerDbV5>(bytes, _serializationOptions, new PropertyVersion("BinnerDbV6", "BinnerDbV7")), (upgradeDb) => BuildChecksum(upgradeDb)),
                    // Version  (upgrade required)
                    BinnerDbV6.VersionNumber => new BinnerDbV7(_serializer.Deserialize<BinnerDbV6>(bytes, _serializationOptions, new PropertyVersion("BinnerDbV7")), (upgradeDb) => BuildChecksum(upgradeDb)),
                    // Current version
                    BinnerDbV7.VersionNumber => _serializer.Deserialize<BinnerDbV7>(bytes, _serializationOptions),
                    _ => throw new InvalidOperationException($"Unsupported database version: {version}"),
                };
                return db;
            }
            catch (Exception ex)
            {
                throw new BinnerConfigurationException($"Failed to load Binner file database!", ex);
            }
        }

        private bool ImportBinnerDb(Binner.Model.Common.IBinnerDb binnerDb, BinnerContext context, int userId)
        {
            var db = binnerDb as BinnerDbV7 ?? throw new Exception("Error - BinnerDb must be Version 7. An upgrade of the database was not performed before migrating to Sqlite.");
            using var transaction = context.Database.BeginTransaction();
            try
            {
                // DisableIdentityInsert not required for Sqlite

                context.Users.Add(new Data.Model.User
                {
                    UserId = userId,
                    Name = "Admin",
                    EmailAddress = "admin",
                    PasswordHash = PasswordHasher.GeneratePasswordHash("admin").GetBase64EncodedPasswordHash(),
                    DateCreatedUtc = DateTime.UtcNow,
                    DateModifiedUtc = DateTime.UtcNow,
                    IsEmailConfirmed = true,
                    IsAdmin = true
                });

                var partTypes = db.PartTypes.GroupBy(x => x.PartTypeId);
                foreach (var e in partTypes)
                {
                    var firstInGroup = e.FirstOrDefault();
                    if (e.Count() > 1)
                        _logger.Warn($"Found a duplicate part type record '{firstInGroup.PartTypeId}'! Filtering out");
                    context.PartTypes.Add(new Data.Model.PartType
                    {
                        PartTypeId = firstInGroup.PartTypeId,
                        ParentPartTypeId = firstInGroup.ParentPartTypeId,
                        Name = firstInGroup.Name,
                        DateCreatedUtc = firstInGroup.DateCreatedUtc,
                        DateModifiedUtc = firstInGroup.DateCreatedUtc,
                        UserId = userId
                    });
                }

                foreach (var e in db.Projects)
                {
                    context.Projects.Add(new Data.Model.Project
                    {
                        Color = e.Color,
                        Description = e.Description,
                        Location = e.Location,
                        Name = e.Name,
                        Notes = e.Notes,
                        ProjectId = e.ProjectId,
                        DateCreatedUtc = e.DateCreatedUtc,
                        DateModifiedUtc = e.DateCreatedUtc,
                        UserId = userId
                    });
                }

                foreach (var e in db.Parts)
                {
                    // ensure destination record exists
                    var partType = db.PartTypes.FirstOrDefault(x => x.PartTypeId == e.PartTypeId);
                    long partTypeId;
                    if (partType == null)
                        partTypeId = (long)SystemDefaults.DefaultPartTypes.Other;
                    else
                        partTypeId = partType.PartTypeId;

                    context.Parts.Add(new Data.Model.Part
                    {
                        PartId = e.PartId,
                        ArrowPartNumber = e.ArrowPartNumber,
                        BinNumber = e.BinNumber,
                        BinNumber2 = e.BinNumber2,
                        Cost = (double)e.Cost,
                        DatasheetUrl = e.DatasheetUrl,
                        Description = e.Description,
                        DigiKeyPartNumber = e.DigiKeyPartNumber,
                        ImageUrl = e.ImageUrl,
                        Keywords = string.Join(",", e.Keywords ?? new List<string>()),
                        Location = e.Location,
                        LowestCostSupplier = e.LowestCostSupplier,
                        LowestCostSupplierUrl = e.LowestCostSupplierUrl,
                        LowStockThreshold = e.LowStockThreshold,
                        Manufacturer = e.Manufacturer,
                        ManufacturerPartNumber = e.ManufacturerPartNumber,
                        MountingTypeId = e.MountingTypeId,
                        MouserPartNumber = e.MouserPartNumber,
                        PackageType = e.PackageType,
                        PartNumber = e.PartNumber,
                        PartTypeId = partTypeId,
                        ProductUrl = e.ProductUrl,
                        ProjectId = e.ProjectId,
                        Quantity = e.Quantity,
                        DateCreatedUtc = e.DateCreatedUtc,
                        DateModifiedUtc = e.DateCreatedUtc,
                        UserId = userId
                    });
                }

                foreach (var e in db.PartSuppliers)
                {
                    if (db.Parts.Any(x => x.PartId == e.PartId))
                    {
                        context.PartSuppliers.Add(new Data.Model.PartSupplier
                        {
                            Cost = e.Cost,
                            DateCreatedUtc = e.DateCreatedUtc,
                            DateModifiedUtc = e.DateModifiedUtc,
                            ImageUrl = e.ImageUrl,
                            MinimumOrderQuantity = e.MinimumOrderQuantity,
                            Name = e.Name,
                            PartId = e.PartId,
                            PartSupplierId = e.PartSupplierId,
                            ProductUrl = e.ProductUrl,
                            QuantityAvailable = e.QuantityAvailable,
                            SupplierPartNumber = e.SupplierPartNumber,
                            UserId = userId
                        });
                    }
                }


                foreach (var e in db.OAuthCredentials)
                {
                    context.OAuthCredentials.Add(new Data.Model.OAuthCredential
                    {
                        AccessToken = e.AccessToken,
                        DateCreatedUtc = e.DateCreatedUtc,
                        DateExpiresUtc = e.DateExpiresUtc,
                        DateModifiedUtc = e.DateCreatedUtc,
                        ApiSettings = "{}",
                        Ip = 0,
                        Provider = e.Provider ?? string.Empty,
                        RefreshToken = e.RefreshToken,
                        UserId = userId
                    });
                }


                foreach (var e in db.OAuthRequests)
                {
                    context.OAuthRequests.Add(new Data.Model.OAuthRequest
                    {
                        AuthorizationCode = e.AuthorizationCode,
                        AuthorizationReceived = e.AuthorizationReceived,
                        Error = e.Error,
                        ErrorDescription = e.ErrorDescription,
                        OAuthRequestId = e.OAuthRequestId,
                        RequestId = e.RequestId,
                        ReturnToUrl = e.ReturnToUrl,
                        DateCreatedUtc = e.DateCreatedUtc,
                        DateModifiedUtc = e.DateCreatedUtc,
                        Ip = 0,
                        Provider = e.Provider,
                        UserId = userId
                    });
                }

                foreach (var e in db.Pcbs)
                {
                    context.Pcbs.Add(new Data.Model.Pcb
                    {
                        Cost = e.Cost,
                        Description = e.Description,
                        LastSerialNumber = e.LastSerialNumber,
                        Name = e.Name,
                        PcbId = e.PcbId,
                        Quantity = e.Quantity,
                        SerialNumberFormat = e.SerialNumberFormat,
                        DateCreatedUtc = e.DateCreatedUtc,
                        DateModifiedUtc = e.DateCreatedUtc,
                        UserId = userId
                    });
                }

                foreach (var e in db.StoredFiles)
                {
                    if (db.Parts.Any(x => x.PartId == e.PartId))
                    {
                        context.StoredFiles.Add(new Data.Model.StoredFile
                        {
                            Crc32 = e.Crc32,
                            FileLength = e.FileLength,
                            FileName = e.FileName,
                            OriginalFileName = e.OriginalFileName,
                            PartId = e.PartId,
                            StoredFileId = e.StoredFileId,
                            StoredFileType = (Binner.Model.StoredFileType)e.StoredFileType,
                            DateCreatedUtc = e.DateCreatedUtc,
                            DateModifiedUtc = e.DateCreatedUtc,
                            UserId = userId
                        });
                    }
                }

                foreach (var e in db.PcbStoredFileAssignments)
                {
                    if (db.StoredFiles.Any(x => x.StoredFileId == e.StoredFileId) && db.Pcbs.Any(x => x.PcbId == e.PcbId))
                    {
                        context.PcbStoredFileAssignments.Add(new Data.Model.PcbStoredFileAssignment
                        {
                            Notes = e.Notes,
                            PcbStoredFileAssignmentId = e.PcbStoredFileAssignmentId,
                            StoredFileId = e.StoredFileId,
                            Name = e.Name,
                            PcbId = e.PcbId,
                            DateCreatedUtc = e.DateCreatedUtc,
                            DateModifiedUtc = e.DateCreatedUtc,
                            UserId = userId
                        });
                    }
                }

                foreach (var e in db.ProjectPartAssignments)
                {
                    if ((e.PartId == null || db.Parts.Any(x => x.PartId == e.PartId))
                        && db.Projects.Any(x => x.ProjectId == e.ProjectId)
                        && (e.PcbId == null || db.Pcbs.Any(x => x.PcbId == e.PcbId)))
                    {
                        context.ProjectPartAssignments.Add(new Data.Model.ProjectPartAssignment
                        {
                            CustomDescription = e.CustomDescription,
                            PartId = e.PartId,
                            PartName = e.PartName,
                            ProjectId = e.ProjectId,
                            ProjectPartAssignmentId = e.ProjectPartAssignmentId,
                            Quantity = e.Quantity,
                            QuantityAvailable = e.QuantityAvailable,
                            ReferenceId = e.ReferenceId,
                            SchematicReferenceId = e.SchematicReferenceId,
                            Notes = e.Notes,
                            PcbId = e.PcbId,
                            DateCreatedUtc = e.DateCreatedUtc,
                            DateModifiedUtc = e.DateCreatedUtc,
                            UserId = userId
                        });
                    }
                }

                foreach (var e in db.ProjectPcbAssignments)
                {
                    if (db.Projects.Any(x => x.ProjectId == e.ProjectId) && db.Pcbs.Any(x => x.PcbId == e.PcbId))
                    {
                        context.ProjectPcbAssignments.Add(new Data.Model.ProjectPcbAssignment
                        {
                            ProjectPcbAssignmentId = e.ProjectPcbAssignmentId,
                            ProjectId = e.ProjectId,
                            PcbId = e.PcbId,
                            DateCreatedUtc = e.DateCreatedUtc,
                            DateModifiedUtc = e.DateCreatedUtc,
                            UserId = userId
                        });
                    }
                }

                context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.Error(ex, "Failed to migrate BinnerDbV7 to Sqlite!");
            }
            finally
            {
                // EnableIdentityInsert not required for Sqlite
            }
            return false;
        }

        private void EnableIdentityInsert<T>(BinnerContext context)
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            var schema = entityType.GetSchema();
            if (!string.IsNullOrEmpty(schema))
                schema += ".";
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT {schema}{entityType.GetTableName()} ON;");
        }

        private void DisableIdentityInsert<T>(BinnerContext context)
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            var schema = entityType.GetSchema();
            if (!string.IsNullOrEmpty(schema))
                schema += ".";
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT {schema}{entityType.GetTableName()} OFF;");
        }

        private string BuildChecksum(Binner.Model.Common.IBinnerDb db)
        {
            var bytes = _serializer.Serialize(db, "Checksum");
            using var sha1 = SHA1.Create();
            var hash = Convert.ToBase64String(sha1.ComputeHash(bytes));
            return hash;
        }

        private bool ValidateChecksum(Binner.Model.Common.IBinnerDb db)
        {
            var calculatedChecksum = BuildChecksum(db);
            if (db.Checksum?.Equals(calculatedChecksum) == true)
                return true;
            return false;
        }

        private BinnerDbVersion? ReadDbVersion(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            reader.BaseStream.Position = 0;
            if (reader.BaseStream.Length >= 14)
            {
                var sqliteHeader = reader.ReadBytes(14);
                var header = System.Text.Encoding.UTF8.GetString(sqliteHeader);
                if (header == "SQLite format ")
                    return null;
            }

            reader.BaseStream.Position = 0;
            var versionByte = reader.ReadByte();
            var versionCreated = reader.ReadInt64();
            return new BinnerDbVersion(versionByte, DateTime.FromBinary(versionCreated));
        }
    }
}
