using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;
using WebApi.StructureMap;
using WebApplicationTest.DI;
using WebApplicationTest.Filters;
using WebApplicationTest.Utils;

namespace WebApplicationTest
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            // config.Filters.Add(new MyAuthFilter());

            config.UseStructureMap(cfg =>
            {
                //x.AddRegistry<Registry>();
                //x.AddRegistry<Registry2>();

                
                
                cfg.For<IDateTimeService>().Use<DateTimeService>().Singleton();
                cfg.For<IFilterProvider>().Use<MyFilterProvider>();
                //cfg.For<IAuthenticationFilter>().Use<MyAuthFilter>();
            });



            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
