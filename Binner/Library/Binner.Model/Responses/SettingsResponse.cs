using Binner.Model.Configuration.Integrations;

namespace Binner.Model.Responses
{
    public class SettingsResponse
    {
        /// <summary>
        /// Octopart config
        /// </summary>
        public OctopartUserConfiguration Octopart { get; set; } = new ();

        /// <summary>
        /// Digikey config
        /// </summary>
        public DigiKeyUserConfiguration Digikey { get; set; } = new ();

        /// <summary>
        /// Mouser config
        /// </summary>
        public MouserUserConfiguration Mouser { get; set; } = new ();

        /// <summary>
        /// Arrow config
        /// </summary>
        public ArrowUserConfiguration Arrow { get; set; } = new ();

        /// <summary>
        /// Binner config
        /// </summary>
        public SwarmUserConfiguration Binner { get; set; } = new ();

        /// <summary>
        /// Printer config
        /// </summary>
        public PrinterSettingsResponse Printer { get; set; } = new ();

        /// <summary>
        /// Barcode config
        /// </summary>
        public BarcodeSettingsResponse Barcode { get; set; } = new ();
    }
}
