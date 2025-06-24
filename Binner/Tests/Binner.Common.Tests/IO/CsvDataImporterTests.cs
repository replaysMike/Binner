using Binner.Common.IO;
using Binner.Global.Common;
using Binner.Model;
using Binner.Testing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
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
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
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
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
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
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
        }

        [Test]
        public async Task ShouldImportPartsCsvAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new CsvDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            var files = new List<UploadFile>();
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(@$"#PartId,Quantity,LowStockThreshold,Cost,Currency,PartNumber,DigiKeyPartNumber,MouserPartNumber,ArrowPartNumber,TmePartNumber,Description,PartTypeId,MountingTypeId,PackageType,ProductUrl,ImageUrl,LowestCostSupplier,LowestCostSupplierUrl,ProjectId,Keywords,DatasheetUrl,Location,BinNumber,BinNumber2,Manufacturer,ManufacturerPartNumber,UserId,DateCreatedUtc");
            writer.WriteLine(@$"1,3,10,0.37,""CAD"",""LM358"",""296-1395-5-ND"",""926-LM358AN/NOPB"","""","""",""IC OPAMP GP 2 CIRCUIT 8DIP"",14,1,""8DIP"",""https://www.digikey.ca/en/products/detail/texas-instruments/LM358P/277042"",""https://mm.digikey.com/Volume0/opasdata/d220001/medias/images/6222/296%7E4040082%7EP%7E8.jpg"",""DigiKey"",""https://www.digikey.ca/en/products/detail/texas-instruments/LM358P/277042"",0,""IC OPAMP"",""https://www.ti.com/general/docs/suppproductinfo.tsp?distId=10&gotoUrl=https%3A%2F%2Fwww.ti.com%2Flit%2Fgpn%2Flm358"",""Vancouver"",""1"",""2"",""Texas Instruments"",""LM358"",1,2025-04-24 03:14:48");
            writer.Flush();
            stream.Position = 0;
            files.Add(new UploadFile("Parts.csv", stream));

            var result = await importer.ImportAsync(files, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            Assert.That(result.TotalRowsImported, Is.EqualTo(1));
            Assert.That(result.RowsImportedByTable["Parts"], Is.EqualTo(1));
            Assert.That(db.Parts.Count, Is.EqualTo(1));
            var part = db.Parts.First();
            Assert.That(part.PartNumber, Is.EqualTo("LM358"));
            Assert.That(part.Quantity, Is.EqualTo(3));
            Assert.That(part.Cost, Is.EqualTo(0.37));
            Assert.That(part.Currency, Is.EqualTo("CAD"));
            Assert.That(part.DigiKeyPartNumber, Is.EqualTo("296-1395-5-ND"));
            Assert.That(part.MouserPartNumber, Is.EqualTo("926-LM358AN/NOPB"));
            Assert.That(part.Description, Is.EqualTo("IC OPAMP GP 2 CIRCUIT 8DIP"));
            Assert.That(part.UserId, Is.EqualTo(99));
            CollectionAssert.AreEqual(new[] { "IC", "OPAMP" }, part.Keywords);
        }
    }
}
