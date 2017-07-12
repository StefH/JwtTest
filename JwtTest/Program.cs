using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace JwtTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var payload = new Dictionary<string, object>
            {
                { "sub", "12345667EDBA435@techacct.adobe.com" },
                {  "iss", "8765432DEAB65@AdobeOrg" },
                { "exp", 1473901205 },
                { "aud", "https://ims-na1.adobelogin.com/c/1234-5678-9876-5433" },
                { "https://ims-na1.adobelogin.com/s/ent_campaign_sdk", true }
            };

            var h = new OpenSSLKeyHelper();
            X509Certificate2 certificate = h.GetX509Certificate2(File.ReadAllText("certificate_pub.crt"), File.ReadAllText("private.key"));

            IJwtAlgorithm algorithm = new RS256Algorithm(certificate);
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, "");
            Console.WriteLine(token);
        }
    }
}
