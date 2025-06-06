using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace CraftedSolutions.MarBasAPICore.Auth
{
    public sealed class MapClaimsTransformation(IConfiguration configuration) : IClaimsTransformation
    {
        private readonly IAuthConfig? _authConfig = AuthConfig.Bind(configuration.GetSection(configuration.GetValue(AuthConfig.SectionSwitch, AuthConfig.SectionName)), true);

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (_authConfig is IAuthMappings mappings && !string.IsNullOrEmpty(mappings.MapClaimType) && principal.HasClaim(x => x.Type == mappings.MapClaimType))
            {
                var hasAdditions = false;
                var identity = new ClaimsIdentity(_authConfig.Schema);
                foreach (var c in principal.FindAll(mappings.MapClaimType))
                {
                    _ = mappings.MapRoles.TryGetValue(c.Value, out var v);
                    if (!string.IsNullOrEmpty(v))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, v));
                        hasAdditions = true;
                    }
                }
                if (hasAdditions)
                {
                    principal.AddIdentity(identity);
                }
            }
            return Task.FromResult(principal);
        }
    }
}
