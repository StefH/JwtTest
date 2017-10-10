using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using StructureMap.Attributes;
using WebApplicationTest.Utils;

namespace WebApplicationTest.Filters
{
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        [SetterProperty]
        public IDateTimeService DateTimeService { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            //var context = new HttpAuthenticationContext(actionContext, null);
            HttpRequestMessage request = actionContext.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;
            
            if (authorization == null || authorization.Scheme != "Bearer" || string.IsNullOrEmpty(authorization.Parameter))
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                return;
            }

            if (authorization.Parameter != "xxx")
            {
                //context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, DateTimeService.UtcNow().ToString());
                return;
            }

            var i = new MyIdentity { Name = "name", Sub = 123 };

            actionContext.RequestContext.Principal = new GenericPrincipal(i, new string[] { });
        }
    }
}