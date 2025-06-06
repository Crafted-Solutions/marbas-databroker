using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasAPICore.Auth
{
    public interface IAuthMappings
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        string? MapClaimType { get; set; }
        Dictionary<string, string> MapRoles { get; set; }
    }

    public class AuthMappings : IAuthMappings
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MapClaimType { get; set; }
        public Dictionary<string, string> MapRoles { get; set; } = [];
    }
}
