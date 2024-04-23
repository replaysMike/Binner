using Binner.Common.Extensions;
using NUnit.Framework;
using System.Net;

namespace Binner.Common.Tests.IO
{
    [TestFixture]
    public class IPAddressExtensionsTests
    {
        [TestCase("0.0.0.0", ExpectedResult = 0U, Description = "0.0.0.0")]
        [TestCase("127.0.0.1", ExpectedResult = 2130706433U, Description = "127.0.0.1")]
        [TestCase("192.168.1.55", ExpectedResult = 3232235831U, Description = "192.168.1.55")]
        [TestCase("54.22.161.99", ExpectedResult = 907452771U, Description = "54.22.161.99")]
        [TestCase("12.24.36.48", ExpectedResult = 202908720U, Description = "12.24.36.48")]
        [TestCase("1.1.1.1", ExpectedResult = 16843009U, Description = "1.1.1.1")]
        [TestCase("255.255.255.255", ExpectedResult = 4294967295U, Description = "255.255.255.255")]
        [Test]
        public uint ShouldIpToInt(string ipAddressStr)
        {
            var ipAddress = IPAddress.Parse(ipAddressStr);
            var ip = ipAddress.ToUInt();
            return ip;
        }

        [TestCase(0U, ExpectedResult = "0.0.0.0")]
        [TestCase(2130706433U, ExpectedResult = "127.0.0.1")]
        [TestCase(3232235831U, ExpectedResult = "192.168.1.55")]
        [TestCase(907452771U, ExpectedResult = "54.22.161.99")]
        [TestCase(202908720U, ExpectedResult = "12.24.36.48")]
        [TestCase(16843009U, ExpectedResult = "1.1.1.1")]
        [TestCase(4294967295U, ExpectedResult = "255.255.255.255")]
        [Test]
        public string ShouldIntToIp(uint ip)
        {
            var ipAddress = ip.ToIpAddress();
            return ipAddress.ToString();
        }
    }
}
