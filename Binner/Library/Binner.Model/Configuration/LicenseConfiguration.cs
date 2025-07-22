namespace Binner.Model.Configuration
{
    public class LicenseConfiguration
    {
        private string? _licenseKey = string.Empty;
        /// <summary>
        /// Registered license key for activating paid features
        /// </summary>
        public string? LicenseKey
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.LicenseKey)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.LicenseKey);
                return _licenseKey;
            }
            set
            {
                _licenseKey = value;
            }
        }
    }
}
