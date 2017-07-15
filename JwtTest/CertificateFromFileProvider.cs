using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace JwtTest
{
    class CertificateFromFileProvider : BaseCertificateProvider, ICertificateProvider
    {
        public X509Certificate2 GetCertificate()
        {
            string certificateText = File.ReadAllText(@"c:\temp\certificate_pub.crt");
            string privateKeyText = File.ReadAllText(@"c:\temp\private.key");

            byte[] certBytes = GetCertificateBytes(certificateText);
            X509Certificate2 certificate = new X509Certificate2(certBytes);

            byte[] privateBytes = GetPrivateBytes(privateKeyText);
            certificate.PrivateKey = DecodePrivateKeyInfo(privateBytes);

            return certificate;
        }
    }
}