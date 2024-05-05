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
    public class ExcelDataImporterTests
    {
        [Test]
        public async Task ShouldImportExcelAsync()
        {
            using var storageProvider = new InMemoryStorageProvider(true);
            var importer = new ExcelDataImporter(storageProvider);
            var userContext = new UserContext { UserId = 99 };

            using var stream = new FileStream(".\\IO\\BinnerParts.xlsx", FileMode.Open);
            var result = await importer.ImportAsync("BinnerParts.xlsx", stream, userContext);

            var db = await storageProvider.GetDatabaseAsync(userContext);
            Assert.That(result.Success, Is.True);
            // data based on test set in BinnerParts.xlsx
            Assert.That(result.TotalRowsImported, Is.EqualTo(204));
            Assert.That(result.RowsImportedByTable["Projects"], Is.EqualTo(2));
            Assert.That(result.RowsImportedByTable["Parts"], Is.EqualTo(1));
            Assert.That(result.RowsImportedByTable["PartTypes"], Is.EqualTo(201));
            Assert.That(db.Projects.Count, Is.EqualTo(2));
            Assert.That(db.Parts.Count, Is.EqualTo(1));
            Assert.That(db.PartTypes.Count, Is.EqualTo(205));
            Assert.That(db.Projects.First().UserId, Is.EqualTo(99));
        }
    }
}
