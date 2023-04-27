using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Responses;

namespace Binner.Model.Requests
{
    public class SettingsRequest
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
        /// Binner swarm config
        /// </summary>
        public SwarmUserConfiguration Binner { get; set; } = new ();

        /// <summary>
        /// Printer configuration
        /// </summary>
        public PrinterSettingsResponse Printer { get; set; } = new ();

        /// <summary>
        /// Barcode configuration
        /// </summary>
        public BarcodeConfiguration Barcode { get; set; } = new();
    }
}
