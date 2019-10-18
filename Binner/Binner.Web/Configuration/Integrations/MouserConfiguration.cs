namespace Binner.Web.Configuration
{
    public class MouserConfiguration
    {
        /// <summary>
        /// Mouser Api Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Path to the Mouser Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.mouser.com";
    }
}
