using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using StructureMap;

namespace WebApplicationTest.DI
{
    public class MyFilterProvider : IFilterProvider
    {
        private IContainer _container;
        private readonly ActionDescriptorFilterProvider _defaultProvider = new ActionDescriptorFilterProvider();

        public MyFilterProvider(IContainer container)
        {
            _container = container;
        }

        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            var attributes = _defaultProvider.GetFilters(configuration, actionDescriptor).ToList();

            foreach (var attr in attributes)
            {
                _container.BuildUp(attr.Instance);
            }

            return attributes;
        }
    }
}