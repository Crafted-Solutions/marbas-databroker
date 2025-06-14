using CraftedSolutions.MarBasAPICore.Routing;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasAPICore.Auth
{
    public class TokenValidationFlags
    {
        public const TokenValidationFlag Default = TokenValidationFlag.LogTokenId | TokenValidationFlag.LogValidationExceptions | TokenValidationFlag.RequireExpirationTime | TokenValidationFlag.RequireSignedTokens | TokenValidationFlag.RequireAudience |
            TokenValidationFlag.TryAllIssuerSigningKeys | TokenValidationFlag.ValidateAudience | TokenValidationFlag.ValidateIssuer | TokenValidationFlag.ValidateLifetime;
        public TokenValidationFlag Values { get; set; } = Default;
        public TokenValidationFlag Set { get => ~Default & Values; set => Values = Default | value; }
        public TokenValidationFlag Unset { get => Default & ~Values; set => Values = Default & ~value; }

        public void Commit<T>(T obj)
        {
            var vals = Enum.GetValues<TokenValidationFlag>();
            var type = typeof(T);
            foreach (var v in vals)
            {
                if (0 != (Set & v))
                {
                    type.GetProperty(Enum.GetName(v)!)?.SetValue(obj, true);
                }
                else if (0 != (Unset & v))
                {
                    type.GetProperty(Enum.GetName(v)!)?.SetValue(obj, false);
                }
            }
        }
    }

    public class OIDCAuthConfigBackend : OIDCAuthConfig, IAuthMappings
    {
        public TokenValidationFlags? TokenValidation { get; set; }
        public bool? RequireHttpsMetadata { get; set; } = true;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MapClaimType { get; set; }
        public Dictionary<string, string> MapRoles { get; set; } = [];

        public OpenApiOAuthFlows GenerateFlows()
        {
            var result = new OpenApiOAuthFlows();
            result.GetType().GetProperty(Enum.GetName(Flow)!)?.SetValue(result, new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri(AuthorizationUrl),
                TokenUrl = new Uri(
                    UseTokenProxy ? $"/{RoutingConstants.DefaultPrefix}/OAuth/Token" : TokenUrl,
                    UseTokenProxy ? UriKind.Relative : UriKind.RelativeOrAbsolute
                    ),
                Scopes = Scopes.ToDictionary(x => x.Key, x => ""),
                RefreshUrl = new Uri(string.IsNullOrEmpty(RefreshUrl) ? TokenUrl : RefreshUrl)
            });
            return result;
        }

    }
}
