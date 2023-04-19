using Binner.Model.Configuration.Integrations;
using Binner.Model.Responses;

namespace Binner.Model.Requests
{
    public class SettingsRequest
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
        /// Binner swarm config
        /// </summary>
        public SwarmUserConfiguration Binner { get; set; } = new SwarmUserConfiguration();

        public PrinterSettingsResponse Printer { get; set; } = new PrinterSettingsResponse();
    }
}
