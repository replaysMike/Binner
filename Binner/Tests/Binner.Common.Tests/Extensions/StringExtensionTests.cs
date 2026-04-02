using Binner.Common.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.Tests.Extensions
{
    [TestFixture]
    public class StringExtensionTests
    {
        [Test]
        public void Should_ContextSensitiveSplit_SplitOnComma()
        {
            var input = "SOT-23,DIP,PDIP,SOT-223";
            var result = input.ContextSensitiveSplit(",");
            Assert.That(result.Length, Is.EqualTo(4));
            Assert.That(result.First(), Is.EqualTo("SOT-23"));
            Assert.That(result.Skip(1).First(), Is.EqualTo("DIP"));
            Assert.That(result.Skip(2).First(), Is.EqualTo("PDIP"));
            Assert.That(result.Skip(3).First(), Is.EqualTo("SOT-223"));
        }

        [Test]
        public void Should_ContextSensitiveSplit_WithParentheses()
        {
            var input = "SOT-23,DIP,PDIP (2mm width, 1mm height),SOT-223";
            var result = input.ContextSensitiveSplit(",");
            Assert.That(result.Length, Is.EqualTo(4));
            Assert.That(result.First(), Is.EqualTo("SOT-23"));
            Assert.That(result.Skip(1).First(), Is.EqualTo("DIP"));
            Assert.That(result.Skip(2).First(), Is.EqualTo("PDIP (2mm width, 1mm height)"));
            Assert.That(result.Skip(3).First(), Is.EqualTo("SOT-223"));
        }

        [Test]
        public void Should_ContextSensitiveSplit_WithParenthesesNoEmptyTrimmed()
        {
            var input = "SOT-23,DIP ,PDIP (2mm width, 1mm height),SOT-223,";
            var result = input.ContextSensitiveSplit(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Assert.That(result.Length, Is.EqualTo(4));
            Assert.That(result.First(), Is.EqualTo("SOT-23"));
            Assert.That(result.Skip(1).First(), Is.EqualTo("DIP"));
            Assert.That(result.Skip(2).First(), Is.EqualTo("PDIP (2mm width, 1mm height)"));
            Assert.That(result.Skip(3).First(), Is.EqualTo("SOT-223"));
        }
    }
}
