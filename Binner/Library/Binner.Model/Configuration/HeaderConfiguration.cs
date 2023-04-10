namespace Binner.Model.Configuration
{
    public class HeaderConfiguration
    {
        /// <summary>
        /// Custom headers to add to each request
        /// </summary>
        public IList<CustomHeader> AddHeaders { get; set; } = new List<CustomHeader>();

        /// <summary>
        /// Fingerprint header ordering by domain.
        /// "Default" key used for the default settings.
        /// </summary>
        public IDictionary<string, IList<string>> HeaderFingerprintConfiguration { get; set; } = new Dictionary<string, IList<string>>();
    }
}
