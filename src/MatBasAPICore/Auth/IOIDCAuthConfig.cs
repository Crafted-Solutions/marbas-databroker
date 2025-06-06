using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasAPICore.Auth
{
    public enum AuthFlow { Implicit, Password, ClientCredentials, AuthorizationCode }
    public enum CapabilitySpec { NA, Available, Required }

    [Flags]
    public enum TokenValidationFlag
    {
        LogTokenId = 0x0001,
        LogValidationExceptions = 0x0002,
        RequireExpirationTime = 0x0004,
        RequireSignedTokens = 0x0008,
        RequireAudience = 0x0010,
        SaveSigninToken = 0x0020,
        TryAllIssuerSigningKeys = 0x0040,
        ValidateActor = 0x0080,
        ValidateAudience = 0x0100,
        ValidateIssuer = 0x0200,
        ValidateIssuerSigningKey = 0x0400,
        ValidateLifetime = 0x0800,
        ValidateTokenReplay = 0x1000
    }

    public interface IOIDCAuthConfig : IAuthConfig
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        string? Audience { get; set; }
        string Authority { get; set; }
        string AuthorizationUrl { get; set; }
        string TokenUrl { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? UserInfoUrl { get; set; }
        string ClientId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        string? ClientSecret { get; set; }
        AuthFlow Flow { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        CapabilitySpec PKCE { get; set; }
        Dictionary<string, bool> Scopes { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        string? ScopeSeparator { get; set; }
        public bool UseTokenProxy { get; set; }
    }
}
