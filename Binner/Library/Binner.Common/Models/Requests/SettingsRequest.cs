using Binner.Common.Models.Configuration;
using Binner.Common.Models.Responses;

namespace Binner.Common.Models.Requests
{
    public class SettingsRequest
    {
        /// <summary>
        /// Octopart config
        /// </summary>
        public OctopartConfiguration Octopart { get; set; } = new OctopartConfiguration();

        /// <summary>
        /// Digikey config
        /// </summary>
        public DigikeyConfiguration Digikey { get; set; } = new DigikeyConfiguration();

        /// <summary>
        /// Mouser config
        /// </summary>
        public MouserConfigurationResponse Mouser { get; set; } = new MouserConfigurationResponse();

        public PrinterSettingsResponse Printer { get; set; } = new PrinterSettingsResponse();
    }
}
