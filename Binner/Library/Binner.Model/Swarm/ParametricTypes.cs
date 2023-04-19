namespace Binner.Model.Swarm
{
    public enum ParametricTypes
    {
        // BinnerType = DigiKey ParameterId
        InputBiasCurrent = 974,         // Current - Input Bias
        InputOffsetVoltage = 975,       // Voltage - Input Offset
        SupplyCurrent = 449,            // Current - Supply, "90µA (x2 Channels)"
        MinSupplyVoltage = 2609,        // Voltage - Supply Span (Min)
        MaxSupplyVoltage = 2610,        // Voltage - Supply Span (Max)
        OutputCurrentPerChannel = 2115, // Current - Output / Channel
        OperatingTemperature = 252,     // Operating Temperature, "0°C ~ 70°C"
        OutputType = 41,                // Output Type, "Differential"
        MountingType = 69,              // Mounting Type, "Through Hole", "Surface Mount", etc
        DevicePackage = 1291,           // Supplier Device Package, 8-DIP (widthmm,heightmm or width", height")
        NumberOfCircuits = 2094,        // Number of Circuits
        Packaging = 7,                  // Packaging, "Tape & Reel (TR)"
        SlewRate = 511,                 // Slew Rate, "1.5V/µs"
        AmplifierType = 161,            // Amplifier Type, "General Purpose"
        GainBandwidthProduct = 658,     // Gain Bandwidth Product, "1 MHz"
        PartStatus = 1989               // Part Status, "Active"
    }
}
