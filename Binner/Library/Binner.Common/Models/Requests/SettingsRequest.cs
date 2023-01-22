using Binner.Common.Models.Configuration.Integrations;
using Binner.Common.Models.Responses;

namespace Binner.Common.Models.Requests
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
        /// Binner swarm config
        /// </summary>
        public SwarmUserConfiguration Binner { get; set; } = new SwarmUserConfiguration();

        public PrinterSettingsResponse Printer { get; set; } = new PrinterSettingsResponse();
    }
}
