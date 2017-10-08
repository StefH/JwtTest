using System.Collections.Generic;
using System.Web.Http;
using WebApplicationTest.Filters;

namespace WebApplicationTest.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        [Authorize]
        public IEnumerable<object> Get()
        {
            var p = RequestContext.Principal;
            var i = (MyIdentity)p.Identity;
            return new object[] { "value1", "value2", i.Name, i.Sub };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
