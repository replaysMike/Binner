using Binner.Common.Models.Configuration.Integrations;

namespace Binner.Common.Models.Responses
{
    public class SettingsResponse
    {
        /// <summary>
        /// Octopart config
        /// </summary>
        public OctopartUserConfiguration Octopart { get; set; } = new OctopartUserConfiguration();

        /// <summary>
        /// Digikey config
        /// </summary>
        public DigiKeyUserConfiguration Digikey { get; set; } = new DigiKeyUserConfiguration();

        /// <summary>
        /// Mouser config
        /// </summary>
        public MouserUserConfiguration Mouser { get; set; } = new MouserUserConfiguration();

        /// <summary>
        /// Arrow config
        /// </summary>
        public ArrowUserConfiguration Arrow { get; set; } = new ArrowUserConfiguration();

        /// <summary>
        /// Binner config
        /// </summary>
        public SwarmUserConfiguration Binner { get; set; } = new SwarmUserConfiguration();

        /// <summary>
        /// Printer config
        /// </summary>
        public PrinterSettingsResponse Printer { get; set; } = new PrinterSettingsResponse();
    }
}
