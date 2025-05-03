using Binner.Common.Services;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace Binner.Common.Tests.Services
{
    [TestFixture]
    public class StoredFileServiceTests
    {
        [Test]
        [TestCase(@"myinvalidfile"".txt", @"LM358""", @"/userfiles@@ProductImage@@LM358-ProductImage.txt")]
        [TestCase(@"my*invalidfile.jpg", @"LM*358", @"/userfiles@@ProductImage@@LM358-ProductImage.jpg")]
        [TestCase(@"my*invalidfile.jpg", @"LM*358", @"/userfiles@@ProductImage@@LM358-ProductImage.jpg")]
        [TestCase(@"my?invalidfile.jpg", @"LM?358", @"/userfiles@@ProductImage@@LM358-ProductImage.jpg")]
        [TestCase("my\tinvalidfile.png", "LM\t358", @"/userfiles@@ProductImage@@LM358-ProductImage.png")]
        [TestCase("myinvalidfile\n.jpg", "LM\n358", @"/userfiles@@ProductImage@@LM358-ProductImage.jpg")]
        [TestCase("myinvalidfile\r.pdf", "LM\r358", @"/userfiles@@ProductImage@@LM358-ProductImage.pdf")]
        [TestCase("myinvalidfile|.jpg", "LM|358", @"/userfiles@@ProductImage@@LM358-ProductImage.jpg")]
        [TestCase("myinvalidfile<>.jpg", "LM<>358", @"/userfiles@@ProductImage@@LM358-ProductImage.jpg")]
        public async Task ShouldNotCreateFilesWithInvalidChars(string testFilename, string partName, string expected)
        {
            var testContext = new TestContext();
            var partService = new StoredFileService(testContext.StorageProvider, testContext.RequestContextAccessor.Object, testContext.StorageProviderConfiguration);
            var part = await testContext.StorageProvider.GetPartAsync("LM358", testContext.RequestContextAccessor.Object.GetUserContext());

            Assert.That(part, Is.Not.Null);

            part.PartNumber = partName;
            var generatedFilename = partService.GenerateFilename(testFilename, part, Model.StoredFileType.ProductImage);

            Assert.That(generatedFilename, Is.EqualTo(expected.Replace("@@", Path.DirectorySeparatorChar.ToString())));
        }
    }
}
