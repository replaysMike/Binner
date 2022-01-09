using System;
using static Binner.Common.Models.SystemDefaults;

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
            Relay,
            Transformer,
            Crystal,
            Sensor,
            Switch,
            Cable,
            Connector,
            IC,
            Hardware,
            Other,
            [Parent(DefaultPartTypes.IC)]
            OpAmp,
            [Parent(DefaultPartTypes.IC)]
            Amplifier,
            [Parent(DefaultPartTypes.IC)]
            Memory,
            [Parent(DefaultPartTypes.IC)]
            Logic,
            [Parent(DefaultPartTypes.IC)]
            Interface,
            [Parent(DefaultPartTypes.IC)]
            Microcontroller,
            [Parent(DefaultPartTypes.IC)]
            Clock,
            [Parent(DefaultPartTypes.IC)]
            ADC,
            [Parent(DefaultPartTypes.IC)]
            VoltageRegulator,
            [Parent(DefaultPartTypes.IC)]
            EnergyMetering,
            [Parent(DefaultPartTypes.IC)]
            LedDriver,
            [Parent(DefaultPartTypes.Transistor)]
            MOSFET,
            [Parent(DefaultPartTypes.Transistor)]
            IGBT,
            [Parent(DefaultPartTypes.Transistor)]
            JFET,
            [Parent(DefaultPartTypes.Transistor)]
            SCR,
            [Parent(DefaultPartTypes.Transistor)]
            DIAC,
            [Parent(DefaultPartTypes.Transistor)]
            TRIAC,
        }
    }

    public class ParentAttribute : Attribute
    {
        DefaultPartTypes Parent { get; }
        public ParentAttribute(DefaultPartTypes parent)
        {
            Parent = parent;
        }
    }

}
