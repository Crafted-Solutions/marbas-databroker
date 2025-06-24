using System.Security.Claims;
using System.Security.Principal;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.AspNetCore.Http;

namespace CraftedSolutions.MarBasAPICore.Http
{
    public sealed class HttpBrokerContext(IHttpContextAccessor httpContextAccessor) : IBrokerContext
    {
        private readonly IPrincipal _user = httpContextAccessor?.HttpContext?.User ?? SchemaDefaults.AnonymousUser;

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
