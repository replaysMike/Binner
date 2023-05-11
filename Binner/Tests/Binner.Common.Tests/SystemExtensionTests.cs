using Binner.Common.Extensions;
using NUnit.Framework;
using System;
using System.IO;

namespace Binner.Common.Tests
{
    [TestFixture]
    public class SystemExtensionTests
    {
        [Test]
        public void Should_ParseDollarCurrency()
        {
            var test = $"$5.50";
            var value = test.FromCurrency();
            Assert.That(value, Is.EqualTo(5.50));
        }

        [Test]
        public void Should_ParseEuroCurrency()
        {
            var test = $"€5,50";
            var value = test.FromCurrency();
            Assert.That(value, Is.EqualTo(5.50));
        }

        [Test]
        public void Should_ParseNoCurrencySymbol()
        {
            var test = $"5.50";
            var value = test.FromCurrency();
            Assert.That(value, Is.EqualTo(5.50));
        }

        [Test]
        public void UriTests()
        {
        }
    }
}