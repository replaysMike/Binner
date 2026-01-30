namespace Binner.Model
{
    /// <summary>
    /// Pinout definition, serialized as json
    /// </summary>
    public class PinoutDefinition
    {
        /// <summary>
        /// List of pins in the pinout
        /// </summary>
        public List<PinDescription> Pins { get; set; } = new();
    }

    public class PinDescription
    {
        /// <summary>
        /// Pin number, 1-based
        /// </summary>
        public int Pin { get; set; }

        /// <summary>
        /// Label that describes the pin's purpose, e.g. "Vout", "GND", "Vs", etc.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The pin's function, e.g. "Temperature Sensor Analog Output"
        /// </summary>
        public string? Function { get; set; }

        /// <summary>
        /// The type of pin, e.g. "Input", "Output", "Bidirectional", "Power", "Ground", etc.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Description of the pin such as "Supply input; connect to 4V-30V"
        /// </summary>
        public string? Description { get; set; }
    }
}
