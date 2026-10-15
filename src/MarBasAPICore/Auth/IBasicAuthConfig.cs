using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasAPICore.Auth
{
    public interface IBasicAuthConfig: IAuthConfig
    {
    }

    public class BasicAuthConfig : AuthConfig, IBasicAuthConfig
    {
        [JsonIgnore]
        public override string Schema => "Basic";
    }
}
