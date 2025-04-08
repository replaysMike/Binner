using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Binner.Common.Security
{
    public static class CertificateGenerator
    {
        /// <summary>
        /// Create a new self-signed certificate
        /// </summary>
        /// <param name="subjectName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static (X509Certificate2 PfxCertificate, X509Certificate2 Certificate, X509ContentType CertType, byte[] PfxByteArray, byte[] CrtByteArray) GenerateSelfSignedCertificate(string subjectName, string? password)
        {
            // Create a new self-signed certificate
            var ecdsa = ECDsa.Create();
            var rsa = RSA.Create(4096);
            var req = new CertificateRequest($"O=Binner,CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
              {
                // server authentication
                new Oid("1.3.6.1.5.5.7.3.1"),
                // client authentication
                new Oid("1.3.6.1.5.5.7.3.2")
              }, critical: false));

            var san = new SubjectAlternativeNameBuilder();
            san.AddIpAddress(IPAddress.Loopback);
            san.AddIpAddress(IPAddress.Parse("0.0.0.0"));
            san.AddIpAddress(IPAddress.IPv6Loopback);
            san.AddDnsName("localhost");
            san.AddDnsName(Environment.MachineName);
            req.CertificateExtensions.Add(san.Build());

            var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));
            if (OperatingSystem.IsWindows())
                cert.FriendlyName = "Binner HTTPS";
            var crtBytes = cert.Export(X509ContentType.Cert, password);
            var pfxBytes = cert.Export(X509ContentType.Pfx, password);
            X509ContentType actualType = X509Certificate2.GetCertContentType(pfxBytes);
            var exportablePfx = X509CertificateLoader.LoadPkcs12(pfxBytes, password, X509KeyStorageFlags.UserKeySet);
            return (exportablePfx, cert, actualType, pfxBytes, crtBytes);
        }

        public static (bool Result, string Error) AddCertificateToStore(ILogger logger, string filename, string? password)
        {
            //Import-PfxCertificate -FilePath ".\Certificates\certificate.pfx" -CertStoreLocation cert:\LocalMachine\Root -Password (ConvertTo-SecureString -String password -Force -AsPlainText)
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    logger.Info($"Attempting to register certificate in Windows store...");
                    var processInfo = new ProcessStartInfo
                    {
                        Verb = "runas",
                        LoadUserProfile = true,
                        FileName = "powershell.exe",
                        Arguments = $"Import-PfxCertificate -FilePath \"{Path.GetFullPath(filename)}\" -CertStoreLocation cert:\\LocalMachine\\Root -Password (ConvertTo-SecureString -String \"{password}\" -Force -AsPlainText)",
                        RedirectStandardOutput = false,
                        UseShellExecute = true,
                        CreateNoWindow = true
                    };

                    var p = Process.Start(processInfo);
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        logger.Error($"Failed to register certificate! Exit code: ${p.ExitCode}");
                        return (false, $"Exit code: ${p.ExitCode}");
                    }
                }
                catch (Exception ex)
                {
                    return (false, ex.GetBaseException().Message);
                }
            }
            else
            {
                // unix
                try
                {
                    logger.Info($"Attempting to register certificate in Unix store (ca-certificates)...");
                    // cp /certificates/binner-docker.crt /usr/local/share/ca-certificates && cp /certificates/binner-docker.crt /usr/local/share/certificates && ls -l /usr/local/share/ca-certificates && ls -l /certificates && head -n 20 /config/appsettings.json
                    // update-ca-certificates
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"cp {filename.Replace(".pfx", ".crt")} /usr/local/share/ca-certificates && cp {filename.Replace(".pfx", ".crt")} /usr/local/share/certificates && update-ca-certificates",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var p = Process.Start(processInfo);
                    p.WaitForExit();
                    if(p.ExitCode != 0)
                    {
                        logger.Error($"Failed to register certificate! Exit code: ${p.ExitCode}");
                        return (false, $"Exit code: ${p.ExitCode}");
                    }
                }
                catch (Exception ex)
                {
                    return (false, ex.GetBaseException().Message);
                }
            }
            return (true, string.Empty);
        }
    }
}
