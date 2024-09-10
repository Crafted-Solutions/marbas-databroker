using System.Security.Claims;
using System.Security.Principal;
using MarBasSchema;
using MarBasSchema.Broker;
using Microsoft.AspNetCore.Http;

namespace MarBasAPICore.Http
{
    public sealed class HttpBrokerContext : IBrokerContext
    {
        private readonly IPrincipal _user;

        public HttpBrokerContext(IHttpContextAccessor httpContextAccessor)
        {
            _user = httpContextAccessor?.HttpContext?.User ?? SchemaDefaults.AnonymousUser;
        }

        public IPrincipal User => _user;

        public IEnumerable<string> UserRoles
        {
            get
            {
                if (_user is ClaimsPrincipal claims)
                {
                    return claims.FindAll(ClaimTypes.Role).Select(x => x.Value);
                }
                return System.Collections.Immutable.ImmutableList<string>.Empty;
            }
        }
    }
}
