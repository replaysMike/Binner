namespace Binner.Web.Configuration
{
    public class AliExpressConfiguration
    {
        /// <summary>
        /// AliExpress Api Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Path to the AliExpress Api
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.aliexpress.com";
    }
}
