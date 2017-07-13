using System.Collections.Generic;
using System.Threading.Tasks;
using RestEase;

namespace JwtTest
{
    public interface IAdobeAccessApi
    {
        [Post]
        Task<AccessModel> AuthorizeAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> body);
    }
}