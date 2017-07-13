using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Serializers;
using RestEase;

namespace JwtTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // JavaScience.opensslkey.Main2(new [] { "private.key" });

            var payload = new Dictionary<string, object>
            {
                { "sub", "12345667EDBA435@techacct.adobe.com" },
                { "iss", "8765432DEAB65@AdobeOrg" },
                { "exp", 1473901205 },
                { "aud", "https://ims-na1.adobelogin.com/c/1234-5678-9876-5433" },
                { "https://ims-na1.adobelogin.com/s/ent_campaign_sdk", true }
            };

            var h = new OpenSSLKeyHelper();
            X509Certificate2 certificate = h.GetX509Certificate2(File.ReadAllText(@"c:\temp\certificate_pub.crt"), File.ReadAllText(@"c:\temp\private.key"));

            IJwtAlgorithm algorithm = new RS256JwtAlgorithm(certificate);
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            string token = encoder.Encode(payload, "");
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
