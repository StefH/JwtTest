using System.Security.Cryptography.X509Certificates;

namespace JwtTest
{
    public interface ICertificateProvider
    {
        X509Certificate2 Certificate { get; }
    }
}