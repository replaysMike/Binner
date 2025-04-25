using Binner.Model;
using Binner.Model.Configuration;
using System;

namespace Binner.Common
{
    /// <summary>
    /// Provides access to system paths by operating system
    /// </summary>
    public static class SystemPaths
    {
        public static string GetUserFilesPath(StorageProviderConfiguration storageConfig)
        {
            if (OperatingSystem.IsWindows())
                return string.IsNullOrEmpty(storageConfig.UserUploadedFilesPath) ? AppConstants.DefaultWindowsUserFilesPath : storageConfig.UserUploadedFilesPath;
            return string.IsNullOrEmpty(storageConfig.UserUploadedFilesPath) ? AppConstants.DefaultUserFilesPath : storageConfig.UserUploadedFilesPath;
        }

        public static string GetCerficiatePath(WebHostServiceConfiguration serviceConfig)
        {
            if (OperatingSystem.IsWindows())
                return string.IsNullOrEmpty(serviceConfig.SslCertificate) ? AppConstants.DefaultWindowsCertificatesPath : serviceConfig.SslCertificate;
            return string.IsNullOrEmpty(serviceConfig.SslCertificate) ? AppConstants.DefaultCertificatesPath : serviceConfig.SslCertificate;
        }
    }
}
