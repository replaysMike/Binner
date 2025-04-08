using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace Binner.Common.Security
{
    public static class CertificateLoader
    {
        /// <summary>
        /// Load a certificate file
        /// </summary>
        /// <param name="certificatePath"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static (X509Certificate2 Certificate, X509ContentType CertType) LoadCertificate(string certificatePath, string? password)
        {
            if (!File.Exists(certificatePath))
            {
                throw new FileNotFoundException($"Certificate at path '{certificatePath}' could not be found.");
            }

            try
            {
                var bytes = File.ReadAllBytes(certificatePath);
                X509ContentType actualType = X509Certificate2.GetCertContentType(bytes);

                // https://stackoverflow.com/questions/15285046/why-do-i-get-an-access-denied-error-when-creating-an-x509certificate2-object
                return (X509CertificateLoader.LoadPkcs12(bytes, password, X509KeyStorageFlags.UserKeySet), actualType);
                //return (new X509Certificate2(bytes, password, X509KeyStorageFlags.UserKeySet), actualType);
                //return (X509CertificateLoader.LoadPkcs12FromFile(certificatePath, password), actualType);
            }
            catch (Exception ex)
            {
                throw new Exception($"The certificate could not be loaded.", ex);
            }
        }
    }
}
