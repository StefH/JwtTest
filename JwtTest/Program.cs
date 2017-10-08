using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Jose;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using RestEase;

namespace JwtTest
{
    class Program
    {
        private static void RunHMACSHA256Algorithm()
        {
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            //var payload = new Dictionary<string, object>
            //{
            //    { "sub", "1234567890" },
            //    { "name", "John Doe" },
            //    { "admin", true }
            //};
            var payload = new PayloadTest
            {
                Sub = "1234567890",
                Name = "John Doe",
                Admin = true
            };

            byte[] secretBytes = Encoding.UTF8.GetBytes("stef");

            Jose.JWT.DefaultSettings.JsonMapper = new NewtonsoftMapper();

            string tokenStef = Jose.JWT.Encode(payload, secretBytes, JwsAlgorithm.HS256);
            string tokenSecret = encoder.Encode(payload, "stef");
            
            PayloadTest decodedPayload;
            try
            {
                decodedPayload = Jose.JWT.Decode<PayloadTest>(tokenStef, secretBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            int y = 0;
        }

        static void Main(string[] args)
        {
            RunHMACSHA256Algorithm();

            // JavaScience.opensslkey.Main2(new [] { "private.key" });

            var payload = new Dictionary<string, object>
            {
                { "sub", "12345667EDBA435@techacct.adobe.com" },
                { "iss", "8765432DEAB65@AdobeOrg" },
                { "exp", 1473901205 },
                { "aud", "https://ims-na1.adobelogin.com/c/1234-5678-9876-5433" },
                { "https://ims-na1.adobelogin.com/s/ent_campaign_sdk", true }
            };

            string certificateText = File.ReadAllText(@"c:\temp\certificate_pub.crt");
            string privateKeyText = File.ReadAllText(@"c:\temp\private.key");
            ICertificateProvider provider = new CertificateFromFileProvider(certificateText, privateKeyText);
            X509Certificate2 certificate = provider.Certificate;

            string token1;
            string token2;
            {
                IJwtAlgorithm algorithm = new RS256JwtAlgorithm(certificate);
                IJsonSerializer serializer = new JsonNetSerializer();
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

                token1 = encoder.Encode(payload, "");
            }
            {
                token2 = Jose.JWT.Encode(payload, certificate.PrivateKey, JwsAlgorithm.RS256);
            }
            var tokenClient = RestClient.For<IAdobeAccessApi>("https://ims-na1.adobelogin.com/ims/exchange/jwt");

            var lines = File.ReadAllLines(@"c:\temp\data.txt");

            var body = new Dictionary<string, object>
            {
                { "client_id", lines[0] },
                { "client_secret", lines[1] },
                { "jwt_token", lines[2] }
            };
            var accessModel = tokenClient.AuthorizeAsync(body).Result;

            var campaignClient = RestClient.For<IAdobeCampaignApi>("https://mc.adobe.io/");
            campaignClient.Authorization = "Bearer " + accessModel.AccessToken;
            campaignClient.ApiKey = lines[3];
            campaignClient.Environment = lines[4];

            string profile = lines[5];
            string resultProfile = campaignClient.RequestProductAsync(profile).Result;

            int p = 0;
        }
    }
}
