using Binner.Common.Integrations;
using Binner.Model;
using NUnit.Framework;

namespace Binner.Common.Tests.Integrations
{
    [TestFixture]
    public class CommonPartValueFormatterTests
    {
        [Test]
        [TestCase("32 V", ParametricUnits.Volt)]
        [TestCase("0°C ~ 70°C", ParametricUnits.TemperatureMetric)]
        [TestCase("0°F ~ 70°F", ParametricUnits.TemperatureImperial)]
        [TestCase("0°K ~ 70°K", ParametricUnits.TemperatureKelvin)]
        [TestCase("500µA", ParametricUnits.Microamp)]
        [TestCase("500uA", ParametricUnits.Microamp)]
        [TestCase("500A", ParametricUnits.Amp)]
        [TestCase("45 nA", ParametricUnits.Nanoamp)]
        [TestCase("2", ParametricUnits.Count)]
        [TestCase("45 Ω", ParametricUnits.Ohms)]
        [TestCase("45 ohms", ParametricUnits.Ohms)]
        [TestCase("4.7kΩ", ParametricUnits.Kiloohm)]
        [TestCase("4.7k", ParametricUnits.Kiloohm)]
        [TestCase("4.7uf", ParametricUnits.Microfarad)]
        [TestCase("4.7nf", ParametricUnits.Nanofarad)]
        [TestCase("4.7f", ParametricUnits.Farad)]
        [TestCase("1H", ParametricUnits.Henry)]
        [TestCase("60hz", ParametricUnits.Hertz)]
        public void ShouldDetectUnitsFromText(string input, ParametricUnits expected)
        {
            var result = CommonPartValueFormatter.DetectUnitsFromText(input);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("32 V", 32)]
        [TestCase("500µA", 500)]
        [TestCase("0°C ~ 70°C", 0)]
        [TestCase("70°C", 70)]
        [TestCase("8-PDIP", 8)]
        public void ShouldDetectValueFromText(string input, double expected)
        {
            var result = CommonPartValueFormatter.DetectValueFromText(input);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
