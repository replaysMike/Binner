using Binner.Common.IO;
using Binner.Global.Common;
using Binner.Testing;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Tests.IO
{
    [TestFixture]
    public class SqlDataImporterTests
    {
        [Test]
        public async Task ShouldImportSqlAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(@$"INSERT INTO Projects (ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES (1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("testfile.sql", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(1));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(1));
            Assert.That(db.Projects.Count, Is.EqualTo(1));
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
        }

        [Test]
        public async Task ShouldImportQuotedDelimiterSqlAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(@$"INSERT INTO Projects (ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES (1, 'Test Project 1', 'test, description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("testfile.sql", stream, userContext);

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
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(@$"INSERT INTO Projects (ProjectId, UserId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES (1, 1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("testfile.sql", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(1));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(1));
            Assert.That(db.Projects.Count, Is.EqualTo(1));
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
        }

        [Test]
        public async Task ShouldImportSqlQuotedAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(@$"INSERT INTO ""Projects"" (""ProjectId"", Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES (1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("testfile.sql", stream, userContext);

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
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            // try insert's with no line-break
            writer.Write(@$"INSERT INTO PartTypes (ParentPartTypeId, Name, DateCreatedUtc) VALUES (1, 'Custom Type 1', '2022-01-01 00:00:00');");
            writer.Write(@$"INSERT INTO PartTypes (ParentPartTypeId, Name, DateCreatedUtc) VALUES (null, 'Custom Type 2', '2022-01-01 00:00:00');");
            // insert with line-breaks
            writer.WriteLine(@$"INSERT INTO Projects (Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES ('Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.WriteLine(@$"INSERT INTO Projects (Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES ('Test Project 2', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.WriteLine(@$"INSERT INTO Projects (Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES ('Test Project 3', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.WriteLine(@$"INSERT INTO Projects (Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES ('Test Project 4', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            // try insert with quoted line-break content
            writer.WriteLine(@$"INSERT INTO Projects (Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES ('Test Project 5', 'test description
more description
more text', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("testfile.sql", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(7));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(5));
            Assert.That(result.RowsImportedByTable["PartTypes"], Is.EqualTo(2));
            Assert.That(db.Projects.Count, Is.EqualTo(5));
            Assert.That(db.PartTypes.Count, Is.EqualTo(9));
        }

        [Test]
        public async Task ShouldImportSqlWithSchemaAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(@$"INSERT INTO dbo.Projects (ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES (1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("testfile.sql", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(1));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(1));
            Assert.That(db.Projects.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task ShouldNotImportSqlWithUnsupportedTableAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(@$"INSERT INTO SomeOtherTable (ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES (1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("testfile.sql", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.False);
            Assert.That(result.TotalRowsImported, Is.EqualTo(0));
        }

        [Test]
        public async Task ShouldNotImportSqlWithUnsupportedSchemaAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(@$"INSERT INTO test.Projects (ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES (1, 'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("testfile.sql", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.False);
            Assert.That(result.TotalRowsImported, Is.EqualTo(0));
            Assert.That(result.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task ShouldNotImportSqlWithInvalidContentAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc");
            writer.WriteLine($@"'Test Project 1', 'test description', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00'");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("SomeTable.sql", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.False);
            Assert.That(result.TotalRowsImported, Is.EqualTo(0));
            Assert.That(result.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task ShouldImportSqlWithUnescapedLineBreaks()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new SqlDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(@$"INSERT INTO Projects (ProjectId, Name, Description, Location, Color, DateCreatedUtc, DateModifiedUtc) VALUES (1, 'Test Project 1', 'test description

more text
something else', 'location', 1, '2022-01-01 00:00:00', '2022-01-01 00:00:00');");
            writer.Flush();
            stream.Position = 0;

            var result = await importer.ImportAsync("testfile.sql", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(1));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(1));
            Assert.That(db.Projects.Count, Is.EqualTo(1));
        }
    }
}
