using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasAPICore.Auth
{
    public class BasicAuthConfigBackend : BasicAuthConfig, IAuthMappings
    {
        private readonly Dictionary<string, byte[]> _pwHashes = [];

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? UserIdClaimType { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MapClaimType { get; set; }
        public Dictionary<string, string> MapRoles { get; set; } = [];
        public Dictionary<string, string> Principals { get; set; } = [];

        public byte[]? GetPasswordHash(string principal)
        {
            if ((!_pwHashes.TryGetValue(principal, out var result) || null == result)
                && (Principals.TryGetValue(principal, out var strHash) && null != strHash))
            {
                result = Convert.FromHexString(strHash);
                _pwHashes.Add(principal, result);
            }
            return null == result && "*" != principal ? GetPasswordHash("*") : result;
        }
    }
}
