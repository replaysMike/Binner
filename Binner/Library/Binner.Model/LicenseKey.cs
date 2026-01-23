namespace Binner.Model
{
    public class LicenseKey
    {
        /// <summary>
        /// The subscription name/level associated with the key
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The license key associated with the subscription
        /// </summary>
        public string Key { get; set; } = string.Empty;
    }
}
