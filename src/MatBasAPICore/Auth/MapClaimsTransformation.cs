using CraftedSolutions.MarBasSchema;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace CraftedSolutions.MarBasAPICore.Auth
{
    public sealed class MapClaimsTransformation(IConfiguration configuration) : IClaimsTransformation
    {
        private readonly IAuthConfig? _authConfig = AuthConfig.Bind(configuration.GetSection(configuration.GetValue(AuthConfig.SectionSwitch, AuthConfig.SectionName)), true);

        private static readonly string[] NameClaimTypeCandidates = [ClaimTypes.NameIdentifier, "preferred_username", "oid", "sub", ClaimTypes.Upn, "ueid"];

        static MapClaimsTransformation()
        {
            var defaultSelector = ClaimsPrincipal.PrimaryIdentitySelector;
            ClaimsPrincipal.PrimaryIdentitySelector = (IEnumerable<ClaimsIdentity> identities) =>
            {
                ArgumentNullException.ThrowIfNull(identities);
                foreach (var id in identities)
                {
                    if (true == id?.HasClaim(x => SchemaDefaults.UserIdentifierClaimType == x.Type))
                    {
                        return id;
                    }
                }
                return defaultSelector?.Invoke(identities);
            };
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var mappings = _authConfig as IAuthMappings;
            if (null != mappings && !string.IsNullOrEmpty(mappings.MapClaimType) && principal.HasClaim(x => x.Type == mappings.MapClaimType))
            {
                var hasAdditions = false;
                var identity = new ClaimsIdentity(_authConfig?.Schema);
                foreach (var c in principal.FindAll(mappings.MapClaimType))
                {
                    if (mappings.MapRoles.TryGetValue(c.Value, out var v) && !string.IsNullOrEmpty(v))
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

            if (!principal.HasClaim(x => SchemaDefaults.UserIdentifierClaimType == x.Type))
            {
                var idClaim = principal.FindFirstValue(mappings?.UserIdClaimType ?? ClaimTypes.Email);
                if (string.IsNullOrEmpty(idClaim))
                {
                    var issClaim = principal.FindFirstValue("iss") ?? principal.FindFirstValue("aud") ?? "unknown";

                    for (var i = 0; string.IsNullOrEmpty(idClaim) && i < NameClaimTypeCandidates.Length; i++)
                    {
                        idClaim = principal.FindFirstValue(NameClaimTypeCandidates[i]);
                    }
                    if (!string.IsNullOrEmpty(idClaim))
                    {
                        idClaim = $"{idClaim}@{CompactIssuer(issClaim)}";
                    }
                }
                if (!string.IsNullOrEmpty(idClaim))
                {
                    principal.AddIdentity(new ClaimsIdentity(new[] { new Claim(SchemaDefaults.UserIdentifierClaimType, idClaim) },
                        _authConfig?.Schema, SchemaDefaults.UserIdentifierClaimType, null));
                }
            }
            return Task.FromResult(principal);
        }

        private static string CompactIssuer(string issuer)
        {
            if (!issuer.StartsWith("http:") && !issuer.StartsWith("https:"))
            {
                return issuer;
            }
            return new Uri(issuer).Host;
        }
    }
}
