namespace Binner.Model.Swarm
{
    /// <summary>
    /// Indicates the image type
    /// </summary>
    public enum ImageTypes
    {
        Unknown,
        /// <summary>
        /// Full page screenshot
        /// </summary>
        FullPage,
        /// <summary>
        /// A product shot of the part
        /// </summary>
        ProductShot,
        /// <summary>
        /// Device pinout
        /// </summary>
        Pinout,
        /// <summary>
        /// Schematic
        /// </summary>
        Schematic,
        /// <summary>
        /// Functional Block Diagram
        /// </summary>
        BlockDiagram,
        /// <summary>
        /// Characteristics chart
        /// </summary>
        Characteristics,
        /// <summary>
        /// Application circuit example
        /// </summary>
        Circuit,
        /// <summary>
        /// Layout example
        /// </summary>
        Layout,
        /// <summary>
        /// Dimensions
        /// </summary>
        Dimensions,
        /// <summary>
        /// Package information
        /// </summary>
        Package
    }
}
