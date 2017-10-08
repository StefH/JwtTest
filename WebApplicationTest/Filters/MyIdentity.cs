using System.Security.Principal;

namespace WebApplicationTest.Filters
{
    public class MyIdentity : IIdentity
    {
        public string Name { get; set; }
        public int Sub { get; set; }
        public string AuthenticationType { get; } = "MyIdentity";
        public bool IsAuthenticated { get; } = true;
    }
}