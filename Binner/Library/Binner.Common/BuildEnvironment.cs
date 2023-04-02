using System;
using System.Configuration;

namespace Binner.Common
{
    /// <summary>
    /// Build Environment tools
    /// </summary>
    public static class BuildEnvironment
    {
        /// <summary>
        /// Get the current build environment information
        /// </summary>
        /// <returns></returns>
        public static EnvironmentData GetBuildEnvironment()
        {
            var environmentName = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Machine)
                ?? System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.User) ?? "Development";
            var buildVersion = System.Environment.GetEnvironmentVariable("BUILD_VERSION", EnvironmentVariableTarget.Machine)
                ?? System.Environment.GetEnvironmentVariable("BUILD_VERSION", EnvironmentVariableTarget.User) ?? string.Empty;
            var buildCommit = System.Environment.GetEnvironmentVariable("BUILD_COMMIT", EnvironmentVariableTarget.Machine)
                ?? System.Environment.GetEnvironmentVariable("BUILD_COMMIT", EnvironmentVariableTarget.User) ?? string.Empty;
#if DEBUG
            if (string.IsNullOrEmpty(environmentName))
                environmentName = "Development";
            if (string.IsNullOrEmpty(buildVersion))
                buildVersion = "1.0.0.0";
            if (string.IsNullOrEmpty(buildCommit))
                buildCommit = "(none)";
#endif
            return new EnvironmentData(environmentName, buildVersion, buildCommit);
        }

        /// <summary>
        /// Validate that the build environment configuration is valid
        /// </summary>
        /// <returns></returns>
        public static EnvironmentData ValidateEnvironment()
        {
            var environmentData = GetBuildEnvironment();
            if (string.IsNullOrEmpty(environmentData.EnvironmentName))
                throw new ConfigurationErrorsException($"Error: The application environment has not been configured. Please set the environment variable 'ASPNETCORE_ENVIRONMENT' to indicate the runtime environment.");
            return environmentData;
        }
    }
}
