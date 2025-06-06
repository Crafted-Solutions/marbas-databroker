using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasAPICore.Auth
{
    public class OIDCAuthConfig : AuthConfig, IOIDCAuthConfig
    {
        public required string Authority { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Audience { get; set; }
        public required string AuthorizationUrl { get; set; }
        public required string TokenUrl { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? RefreshUrl { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? LogoutUrl { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? UserInfoUrl { get; set; }
        public string ClientId { get; set; } = "databroker";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? ClientSecret { get; set; }
        public Dictionary<string, bool> Scopes { get; set; } = [];
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? ScopeSeparator { get; set; }
        public AuthFlow Flow { get; set; } = AuthFlow.AuthorizationCode;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public CapabilitySpec PKCE { get; set; } = CapabilitySpec.NA;
        public bool UseTokenProxy { get; set; }
    }
}
