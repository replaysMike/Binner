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
        public int Width { get; set; } = 640;
        public int Height { get; set; } = 640;
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
        /// The pin's functions, e.g. "Temperature Sensor Analog Output"
        /// </summary>
        public List<PinFunction> Functions { get; set; } = new();

        /// <summary>
        /// For GPIO pins, indicator if it supports analog i/o
        /// </summary>
        public bool IsAnalog { get; set; }

        /// <summary>
        /// True if it is a GPIO pin
        /// </summary>
        public bool IsGPIO { get; set; }

        /// <summary>
        /// The type of pin, e.g. "Input", "Output", "Bidirectional", "Power", "Ground", etc.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Description of the pin such as "Supply input; connect to 4V-30V"
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The bounds of the pin region
        /// </summary>
        public RectBounds Bounds { get; set; } = new();
    }

    public class PinFunction
    {
        /// <summary>
        /// The name of the function
        /// </summary>
        public string Function { get; set; } = string.Empty;

        /// <summary>
        /// A color to assign the function
        /// </summary>
        public string Color { get; set; } = string.Empty;
    }

    public class RectBounds
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double XPerc { get; set; }
        public double YPerc { get; set; }
        public double WidthPerc { get; set; }
        public double HeightPerc { get; set; }
    }
}
