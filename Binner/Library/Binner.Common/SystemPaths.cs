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

        /// <summary>
        /// Ensure that a the url passed is a valid http/https formatted url
        /// </summary>
        /// <param name="url">A potential website url</param>
        /// <returns>null if invalid</returns>
        public static string? EnsureValidAbsoluteHttpUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return url;

            try
            {
                if (url.StartsWith("//")) url = "https:" + url;
                if (!url.StartsWith("http:") && !url.StartsWith("https:")) url = "https://" + url;
                var uri = new Uri(url);

                return uri.ToString();
            }
            catch (UriFormatException) { }
            return null;
        }
    }
}
