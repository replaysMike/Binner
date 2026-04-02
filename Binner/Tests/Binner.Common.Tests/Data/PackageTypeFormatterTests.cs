using Binner.Common.Data;
using NUnit.Framework;

namespace Binner.Common.Tests.Data
{
    [TestFixture]
    public class PackageTypeFormatterTests
    {
        [Test]
        [TestCase("\",3.90mm Width) SOT-23", ExpectedResult = "SOT-23")]
        [TestCase("\",3.90mm Width) ", ExpectedResult = "")]
        [TestCase("SOT-23 (0.154\",3.90mm Width)", ExpectedResult = "SOT-23")]
        [TestCase("SOT-23-3 (0.154\",3.90mm Width)", ExpectedResult = "SOT-23")]
        [TestCase("SOT-23-8 Thin", ExpectedResult = "SOT-23")]
        [TestCase("sot-23-8", ExpectedResult = "SOT-23")]
        [TestCase("8-SOIC (0.154\",3.90mm Width)", ExpectedResult = "SOIC")]
        [TestCase("8-Lead Small Outline IC", ExpectedResult = "SOIC")]
        [TestCase("SOIC-16", ExpectedResult = "SOIC")]
        [TestCase("8-SOIC (0.154\"", ExpectedResult = "SOIC")]
        [TestCase("8-VSSOP", ExpectedResult = "VSSOP")]
        [TestCase("TSOT-23-8", ExpectedResult = "TSOT-23")]
        [TestCase("TO-263AA", ExpectedResult = "TO-263AA")]
        [TestCase("TO-263AA with leads", ExpectedResult = "TO-263AA")]
        [TestCase("D2PAK (2 Leads + Tab)", ExpectedResult = "D2PAK")]
        [TestCase("D2PAK (3 Leads + Tab)", ExpectedResult = "D2PAK")]
        [TestCase("D2PAK (3 Leads + Tab)", ExpectedResult = "D2PAK")]
        [TestCase("TO-92-3 Long Body", ExpectedResult = "TO-92")]
        [TestCase("SC-63 extra", ExpectedResult = "DPAK")]
        [TestCase("TO-252A", ExpectedResult = "TO-252A")]
        [TestCase("20-UFBGA", ExpectedResult = "UFBGA")]
        [TestCase("TO-39-3 Metal Can", ExpectedResult = "TO-39")]
        [TestCase("TO-92-3 (TO-226AA)", ExpectedResult = "TO-92")]
        [TestCase("16-WQFN Exposed Pad", ExpectedResult = "WQFN")]
        [TestCase("20-WFBGA", ExpectedResult = "WFBGA")]
        [TestCase("8-Lead SOIC", ExpectedResult = "SOIC")]
        [TestCase("8-MSOP", ExpectedResult = "VSSOP")]
        [TestCase("8-DIP", ExpectedResult = "DIP")]
        [TestCase("8-Lead PDIP", ExpectedResult = "DIP")]
        [TestCase("8-DIP (0.300\"", ExpectedResult = "DIP")]
        [TestCase("8-Pin Plastic Dual In-Line Package", ExpectedResult = "DIP")]
        [TestCase("TO-220-3 Full Pack", ExpectedResult = "TO-220")]
        [TestCase("TO-252 (DPAK)", ExpectedResult = "DPAK")]
        [TestCase("TO-263-4", ExpectedResult = "D2PAK")]
        [TestCase("TO-92-3 (TO-226AA) Formed Leads", ExpectedResult = "TO-92")]
        [TestCase("D²PAK", ExpectedResult = "D2PAK")]
        [TestCase("TO-205AD Formed Leads", ExpectedResult = "TO-39")]
        [TestCase("TO-220-3 Full Pack", ExpectedResult = "TO-220")]
        [TestCase("0402", ExpectedResult = "0402")]
        [TestCase("TO-263-3, D2PAK (2 Leads + Tab), TO-263AB", ExpectedResult = "D2PAK")]
        [TestCase("TO-252-3, DPAK (2 Leads + Tab), SC-63", ExpectedResult = "DPAK")]
        [TestCase("DPAK (2 Leads + Tab), SC-63, TO-252-3", ExpectedResult = "DPAK")]
        [TestCase("TO-261-4, TO-261AA", ExpectedResult = "SOT-223")]
        [TestCase("3-PowerFLEX™", ExpectedResult = "PowerFLEX")]
        [TestCase("6-uSMD™", ExpectedResult = "uSMD")]
        [TestCase("Formed Leads", ExpectedResult = "")]
        [TestCase("TO-261", ExpectedResult = "SOT-223")]
        [TestCase("TO-263", ExpectedResult = "D2PAK")]
        [TestCase("TO-252", ExpectedResult = "DPAK")]
        public string Should_NormalizePackageType(string packageName)
        {
            return PackageTypeFormatter.FormatPackageName(packageName);
        }

        [TestCase("SOT-23-3", ExpectedResult = "SOT-23 (3)")]
        [TestCase("8-Lead SOIC", ExpectedResult = "SOIC (8)")]
        [TestCase("TO-220-3 Full Pack", ExpectedResult = "TO-220 (3)")]
        [TestCase("TO-92-3 (TO-226AA)", ExpectedResult = "TO-92 (3)")]
        [TestCase("SOIC-16", ExpectedResult = "SOIC (16)")]
        [TestCase("16-SOIC", ExpectedResult = "SOIC (16)")]
        [TestCase("0402", ExpectedResult = "0402 (2)")]
        [TestCase("0805", ExpectedResult = "0805 (2)")]
        [TestCase("6-VFBGA", ExpectedResult = "VFBGA (6)")]
        [TestCase("INVALIDTYPE1,INVALIDTYPE2", ExpectedResult = "INVALIDTYPE1")]
        public string Should_ParsePackageType(string packageName)
        {
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            return parsedPackage.ToString();
        }

        [Test]
        public void Should_ParsePackageSot_23_5()
        {
            var packageName = "SOT-23-5";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("SOT-23"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(5));
            Assert.That(parsedPackage.AlternateNotation, Is.EqualTo("SOT-23-5"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("SC-74A"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-236"));
        }

        [Test]
        public void Should_ParsePackage_D2PAK()
        {
            var packageName = "D2PAK";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("D2PAK"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(3));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-263"));
        }

        [Test]
        public void Should_ParsePackage_D2PAK_WithLeads()
        {
            var packageName = "D2PAK (3 Leads + Tab)";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("D2PAK"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(4));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-263"));
        }

        [Test]
        public void Should_ParsePackage_D2PAK_With5Leads()
        {
            var packageName = "D2PAK (5 Leads)";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("D2PAK"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(5));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-263"));
        }

        [Test]
        public void Should_ParsePackage_D2PAK_From_TO_263()
        {
            var packageName = "TO-263-4";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("D2PAK"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(4));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-263"));
        }

        [Test]
        public void Should_ParsePackage_TO_73()
        {
            var packageName = "TO-73";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("TO-73"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(12));
        }

        [Test]
        public void Should_ParsePackage_0805()
        {
            var packageName = "0805 (2.0x1.25mm)";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("0805"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(2));
            Assert.That(parsedPackage.AlternateNames, Has.Member("2012 (Metric)"));
        }

        [Test]
        public void Should_ParsePackage_TO_261_4_Multiple()
        {
            var packageName = "TO-261-4, TO-261AA";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("SOT-223"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(4));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-261AA"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-261"));
        }

        [Test]
        public void Should_ParsePackage_TO_205AD_Multiple()
        {
            var packageName = "TO-205AD, TO-39-3 Metal Can";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("TO-39"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(3));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-205AD"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-205AD-3"));
        }

        [Test]
        public void Should_ParsePackage_DPAK2_Multiple()
        {
            var packageName = "TO-263-4, D2PAK (3 Leads + Tab), TO-263AA";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("D2PAK"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(4)); // default for D2PAK is 3, but we have an alternate with 4 pins specified
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-263"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-263-4"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-263AA"));
        }

        [Test]
        public void Should_ParsePackage_D2PAK_Multiple()
        {
            var packageName = "TO-263-3, D2PAK (2 Leads + Tab), TO-263AB";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("D2PAK"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(3));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-263"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-263AB"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-262"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("SOT404"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("DDPAK"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("SMD-220"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("Double Decawatt"));
        }

        [Test]
        public void Should_ParsePackage_InvalidType_Multiple()
        {
            var packageName = "INVALIDTYPE1, INVALIDTYPE2";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("INVALIDTYPE1"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(0));
            Assert.That(parsedPackage.AlternateNames, Has.Member("INVALIDTYPE2"));
        }

        [Test]
        public void Should_ParsePackage_TO_261_4_With_Invalid_Multiple()
        {
            var packageName = "TO-261-4, TO-261AA, INVALIDTYPE";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("SOT-223"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(4));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-261AA"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("TO-261"));
            Assert.That(parsedPackage.AlternateNames, Has.Member("INVALIDTYPE"));
        }

        [Test]
        public void Should_ParsePackage_PowerFLEX()
        {
            var packageName = "3-PowerFLEX™";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("PowerFLEX"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(3));
        }

        [Test]
        public void Should_ParsePackage_uSMD()
        {
            var packageName = "6-usmd (1.13x1.82)";
            var parsedPackage = PackageTypeFormatter.ParsePackageName(packageName);
            Assert.That(parsedPackage.Name, Is.EqualTo("uSMD"));
            Assert.That(parsedPackage.Pins, Is.EqualTo(6));
        }

        [Test]
        public void Should_FilterPackageList()
        {
            var packageNames = "TO-220,SOIC,TO-261,VFBGA,TO-252,TO-263,TO-39,D2PAK,TO-92,DPAK";
            var filteredPackages = PackageTypeFormatter.FilterPackageList(packageNames);
            Assert.That(filteredPackages.Count, Is.EqualTo(8));
            Assert.That(filteredPackages, Has.Member("TO-220"));
            Assert.That(filteredPackages, Has.Member("SOIC"));
            Assert.That(filteredPackages, Has.Member("SOT-223"));
            Assert.That(filteredPackages, Has.Member("VFBGA"));
            Assert.That(filteredPackages, Has.Member("TO-39"));
            Assert.That(filteredPackages, Has.Member("D2PAK"));
            Assert.That(filteredPackages, Has.Member("TO-92"));
            Assert.That(filteredPackages, Has.Member("DPAK"));
        }
    }
}
