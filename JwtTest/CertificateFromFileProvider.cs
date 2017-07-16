using System.Security.Cryptography.X509Certificates;

namespace JwtTest
{
    class CertificateFromFileProvider : BaseCertificateProvider, ICertificateProvider
    {
        public CertificateFromFileProvider(string certificateText, string privateKeyText)
        {
            byte[] certBytes = GetPublicKeyBytes(certificateText);
            Certificate = new X509Certificate2(certBytes)
            {
                PrivateKey = DecodePrivateKey(privateKeyText)
            };
        }

        public X509Certificate2 Certificate { get; }
    }
}