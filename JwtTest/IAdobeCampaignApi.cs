using System.Threading.Tasks;
using RestEase;

namespace JwtTest
{
    public interface IAdobeCampaignApi
    {
        [Header("X-API-Key")]
        string ApiKey { get; set; }

        [Header("Authorization")]
        string Authorization { get; set; }

        [Path("environment")]
        string Environment { get; set; }

        [Post("{environment}/campaign/{profile}")]
        Task<string> RequestProductAsync([Path] string profile);
    }
}