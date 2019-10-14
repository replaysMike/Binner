namespace Binner.Common.Models
{
    /// <summary>
    /// Global default data
    /// </summary>
    public static class SystemDefaults
    {
        /// <summary>
        /// The part should be reordered when it gets below this value
        /// </summary>
        public const int LowStockThreshold = 1;

        /// <summary>
        /// Default part type definitions
        /// </summary>
        public enum DefaultPartTypes
        {
            Resistor = 1,
            Capacitor,
            Inductor,
            Diode,
            LED,
            Transistor,
            Mosfet,
            Relay,
            Transformer,
            Crystal,
            Sensor,
            Switch,
            Cable,
            Connector,
            IC,
            Hardware,
            Other
        }
    }
}
