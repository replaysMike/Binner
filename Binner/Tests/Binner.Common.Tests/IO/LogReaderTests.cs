using Binner.Common.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.Tests.IO
{
    [TestFixture]
    public class LogReaderTests
    {
        [Test]
        public async Task ShouldReadLogInReverseAsync()
        {
            var exampleLog = @"2025-07-22 14:27:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 1|
2025-07-22 14:26:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 2|
2025-07-22 14:25:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 3|
2025-07-22 14:24:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 4|
2025-07-22 14:23:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 5|
2025-07-22 14:22:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 6|";

            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(exampleLog);
            await writer.FlushAsync();
            stream.Seek(0, SeekOrigin.Begin);
            var logReader = new LogReader(stream, Encoding.UTF8);
            var output = new List<string>();
            foreach(var line in logReader)
            {
                output.Add(line);
            }

            // ensure lines are in reverse order
            Assert.That(output.Count, Is.EqualTo(6));
            Assert.That(output.Skip(0).First(), Is.EqualTo("2025-07-22 14:22:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 6|"));
            Assert.That(output.Skip(1).First(), Is.EqualTo("2025-07-22 14:23:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 5|"));
            Assert.That(output.Skip(2).First(), Is.EqualTo("2025-07-22 14:24:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 4|"));
            Assert.That(output.Skip(3).First(), Is.EqualTo("2025-07-22 14:25:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 3|"));
            Assert.That(output.Skip(4).First(), Is.EqualTo("2025-07-22 14:26:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 2|"));
            Assert.That(output.Skip(5).First(), Is.EqualTo("2025-07-22 14:27:0.0|DEBUG|Binner.LicensedProvider.ValidateLicenseAspect|Line 1|"));
        }
    }
}
