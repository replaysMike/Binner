using Binner.Common.Services;
using Binner.Model.Configuration;
using NLog;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Binner.Common.Security
{
    public class CertificateUtility
    {
        private readonly string _configFile;
        private readonly ILogger _logger;

        public CertificateUtility(ILogger logger, string configFile)
        {
            _logger = logger;
            _configFile = configFile;
        }

        public X509Certificate2? LoadOrGenerateSelfSignedCertificate(WebHostServiceConfiguration webHostConfig)
        {
            X509Certificate2? certificate = null;
            var certFilename = SystemPaths.GetCerficiatePath(webHostConfig);
            if (webHostConfig.UseHttps)
            {
                // if the certificate file exists, try to load it
                if (File.Exists(certFilename))
                {
                    try
                    {
                        _logger.Info($"Loading Certificate from '{certFilename}'...");
                        var result = CertificateLoader.LoadCertificate(certFilename, webHostConfig.SslCertificatePassword);
                        certificate = result.Certificate;
                        if (certificate != null)
                        {
                            _logger.Info($"{result.CertType} Certificate loaded from '{certFilename}'");
                            _logger.Info($"Using SSL Certificate: '{certificate.Subject}' '{certificate.FriendlyName}'");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Failed to load SSL certificate at '{certFilename}'. Is the password correct?");
                        throw;
                    }
                }
                else
                {
                    // generate a new certificate
                    var result = GenerateSelfSignedCertificate(webHostConfig);
                    certificate = result.Certificate;
                }
            }
            return certificate;
        }

        public (X509Certificate2? Certificate, CertificateState Status, string Error) GenerateSelfSignedCertificate(WebHostServiceConfiguration webHostConfig, bool throwExceptions = false)
        {
            X509Certificate2? certificate = null;
            var status = CertificateState.None;
            // if forceHttps is enabled, but no certificate is found, generate a certificate
            _logger.Info("ForceHttps is enabled, no certificate specified so a self-signed certificate will be generated.");
            try
            {
                var selfSignedCertificate = CertificateGenerator.GenerateSelfSignedCertificate("localhost", webHostConfig.SslCertificatePassword);
                certificate = selfSignedCertificate.PfxCertificate;
                var certificateFilename = SystemPaths.GetCerficiatePath(webHostConfig);
                var crtFilename = certificateFilename.Replace(".pfx", ".crt", StringComparison.InvariantCultureIgnoreCase);

                if (File.Exists(crtFilename))
                {
                    var message = $"Failed to generate certificate '{crtFilename}'. File already exists, refusing to overwrite.";
                    _logger.Error(message);
                    if (throwExceptions) throw new Exception(message);
                    return (null, CertificateState.Error, message);
                }

                if (File.Exists(certificateFilename))
                {
                    var message = $"Failed to generate certificate '{certificateFilename}'. File already exists, refusing to overwrite.";
                    _logger.Error(message);
                    if (throwExceptions) throw new Exception(message);
                    return (null, CertificateState.Error, message);
                }

                // save the crt
                File.AppendAllBytes(crtFilename, selfSignedCertificate.CrtByteArray);
                _logger.Info($"New self-signed certificate saved to '{crtFilename}'");
                // save the pfx
                File.AppendAllBytes(certificateFilename, selfSignedCertificate.PfxByteArray);
                _logger.Info($"New self-signed certificate saved to '{certificateFilename}'");

                status |= CertificateState.Created;

                // update the config with the new certificate path if it's not set
                if (string.IsNullOrEmpty(webHostConfig.SslCertificate))
                {
                    webHostConfig.SslCertificate = certificateFilename;
                    var settingsService = new SettingsService();
                    settingsService.SaveSettingsAsAsync(webHostConfig, nameof(WebHostServiceConfiguration), _configFile, true);
                }

                // attempt to register the certificate in the store for the given platform
                var storeResult = CertificateGenerator.AddCertificateToStore(_logger, certificateFilename, webHostConfig.SslCertificatePassword);
                if (storeResult.Success)
                {
                    status |= CertificateState.Registered;
                    _logger.Info($"Registered certificate '{certificateFilename}' successfully!");
                }
                else
                {
                    var message = $"Failed to registered certificate '{certificateFilename}'. You will need to register the certificate manually on your platform. Error: {storeResult.Error}";
                    _logger.Warn(message);
                }
            }
            catch (Exception ex)
            {
                status |= CertificateState.Error;
                var message = "Failed to generate a self-signed certificate.";
                _logger.Error(ex, message);
                if (throwExceptions) throw new Exception(message, ex);
            }
            return (certificate, status, string.Empty);
        }
    }
}
