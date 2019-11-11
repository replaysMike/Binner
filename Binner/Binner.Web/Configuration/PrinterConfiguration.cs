using Binner.Common.IO.Printing;

namespace Binner.Web.Configuration
{
    public class PrinterConfiguration
    {
        /// <summary>
        /// Full name of printer
        /// </summary>
        public string PrinterName { get; set; } = "Dymo LabelWriter 450";

        /// <summary>
        /// Label model number
        /// </summary>
        public string LabelName { get; set; } = "30346"; // LW 1/2" x 1 7/8"

        /// <summary>
        /// Label paper source
        /// </summary>
        public LabelSource LabelSource { get; set; } = LabelSource.Default;

        /// <summary>
        /// Font name
        /// </summary>
        public string Font { get; set; } = "Segoe UI";
    }
}
