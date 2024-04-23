using Binner.Common.IO;
using Binner.Global.Common;
using Binner.Testing;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Tests.IO
{

    [TestFixture]
    public class CsvDataImporterTests
    {
        [Test]
        public async Task ShouldImportCsvAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new CsvDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            var files = new List<UploadFile>();
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc");
            writer.WriteLine($@"1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.Flush();
            stream.Position = 0;
            files.Add(new UploadFile("Projects.csv", stream));

            var result = await importer.ImportAsync(files, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(1));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(1));
            Assert.That(db.Projects.Count, Is.EqualTo(1));
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
        }

        [Test]
        public async Task ShouldImportQuotedDelimiterCsvAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new CsvDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            var files = new List<UploadFile>();
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc");
            writer.WriteLine($@"1, 'Test Project 1', 'test, description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.Flush();
            stream.Position = 0;
            files.Add(new UploadFile("Projects.csv", stream));

            var result = await importer.ImportAsync(files, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(1));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(1));
            Assert.That(db.Projects.Count, Is.EqualTo(1));
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
        }

        [Test]
        public async Task ShouldIgnoreUserIdAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new CsvDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc, UserId");
            writer.WriteLine($@"1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00',1");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("Projects.csv", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(1));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(1));
            Assert.That(db.Projects.Count, Is.EqualTo(1));
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
        }

        [Test]
        public async Task ShouldImportMultipleRowsAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new CsvDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc");
            writer.WriteLine($@"1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.WriteLine($@"2, 'Test Project 2', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.WriteLine($@"3, 'Test Project 3', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.WriteLine($@"4, 'Test Project 4', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            // try insert with quoted line-break content
            writer.WriteLine($@"5, 'Test Project 5', 'test description\nsome extra data', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("Projects.csv", stream, userContext);
            
            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(5));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(5));
            Assert.That(db.Projects.Count, Is.EqualTo(5));
        }

        [Test]
        public async Task ShouldNotImportWithUnsupportedTableAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new CsvDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc");
            writer.WriteLine($@"1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("SomeTable.csv", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.False);
            Assert.That(result.TotalRowsImported, Is.EqualTo(0));
            Assert.That(result.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task ShouldSkipInvalidRowAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new CsvDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc");
            writer.WriteLine($@"1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.WriteLine($@"2, 'Invalid Test', 'test description', 'location', 1");
            writer.WriteLine($@"3, 'Test Project 2', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("Projects.csv", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(2));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(2));
            Assert.That(result.Warnings.Count, Is.EqualTo(1));
            Assert.That(db.Projects.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task ShouldImportUnquotedAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new CsvDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc");
            writer.WriteLine($@"1, 'Test Project 1',test description, location, 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.WriteLine($@"2, 'Test Project 2', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("Projects.csv", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(2));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(2));
            Assert.That(db.Projects.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task ShouldImportEncodedLineBreaksAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new CsvDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc");
            writer.WriteLine($@"1, 'Test Project 1', 'test description
This is a test
another test', location, 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.WriteLine($@"2, 'Test Project 2', test description\r\nunquoted strings result in decoded line breaks, 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("Projects.csv", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(2));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(2));
            Assert.That(db.Projects.Count, Is.EqualTo(2));
        }
    }
}
