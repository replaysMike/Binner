using System.ComponentModel;

namespace Binner.Common.Integrations.Models.DigiKey
{
    public enum Parametrics
    {
        Power = 2,
        Tolerance = 3,
        Capacitance = 2049,
        VoltageRating = 2079,
        CurrentRating = 2088,
        Inductance = 2087,
        Resistance = 2085,
        MountingType = 69,
    }

    public enum Tolerances
    {
        [Description("0.001%")]
        Percent0001 = 415,
        [Description("0.002%")]
        Percent0002 = 416,
        [Description("0.0025%")]
        Percent00025 = 417,
        [Description("0.005%")]
        Percent0005 = 291,
        [Description("0.01%")]
        Percent001 = 431,
        [Description("0.05%")]
        Percent005 = 357,
        [Description("0.02%")]
        Percent002 = 380,
        [Description("0.1%")]
        Percent01 = 370,
        [Description("0.25%")]
        Percent025 = 377,
        [Description("0.5%")]
        Percent05 = 355,
        [Description("1%")]
        Percent1 = 1,
        [Description("2%")]
        Percent2 = 4,
        [Description("3%")]
        Percent3 = 105,
        [Description("5%")]
        Percent5 = 2,
        [Description("10%")]
        Percent10 = 3,
    }

    public enum Power
    {
        [Description("1/16")]
        SixteenthWatt = 1,
        [Description("1/10")]
        TenthWatt = 2,
        [Description("1/8")]
        EighthWatt = 3,
        [Description("1/4")]
        QuarterWatt = 4,
        [Description("1/2")]
        HalfWatt = 5,
        [Description("1")]
        OneWatt = 6,
        [Description("2")]
        TwoWatt = 7,
        [Description("3")]
        ThreeWatt = 8,
        [Description("5")]
        FiveWatt = 9,
    }
}
