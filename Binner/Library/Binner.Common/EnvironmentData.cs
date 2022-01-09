namespace Binner.Common
{
    /// <summary>
    /// Information about a build environment
    /// </summary>
    public class EnvironmentData
    {
        /// <summary>
        /// The environment name
        /// Provided by system environment variable ASPNETCORE_ENVIRONMENT
        /// </summary>
        public string EnvironmentName { get; set; }
        /// <summary>
        /// The environment build version
        /// Provided by system environment variable BUILD_VERSION
        /// </summary>
        public string BuildVersion { get; set; }

        /// <summary>
        /// The environment git commit
        /// Provided by system environment variable BUILD_COMMIT
        /// </summary>
        public string GitCommit { get; set; }

        public EnvironmentData(string environmentName, string buildVersion, string gitCommit)
        {
            EnvironmentName = environmentName;
            BuildVersion = buildVersion;
            GitCommit = gitCommit;
        }

        public override string ToString()
        {
            return $"{EnvironmentName} {BuildVersion} {GitCommit}";
        }
    }
}
