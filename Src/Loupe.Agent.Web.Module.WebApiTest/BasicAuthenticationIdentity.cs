using System.Security.Principal;

namespace Loupe.Agent.Web.Module.MVCTest
{
    public class BasicAuthenticationIdentity : GenericIdentity
    {
        public BasicAuthenticationIdentity(string name, string password)
            : base(name, "Basic")
        {
            this.Password = password;
        }

        public string Password { get; set; }
    }
}