using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasAPICore.Auth
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "schema")]
    [JsonDerivedType(typeof(OIDCAuthConfig), typeDiscriminator: "OIDC")]
    [JsonDerivedType(typeof(BasicAuthConfig), typeDiscriminator: "Basic")]
    public interface IAuthConfig
    {
        [JsonIgnore]
        string Schema { get; }
    }

    public abstract class AuthConfig : IAuthConfig
    {
        public const string SectionName = "Auth";
        public const string SectionSwitch = "UseAuth";

        [JsonIgnore]
        public abstract string Schema { get; }

        public static IAuthConfig? Bind(IConfiguration configuration, bool backend = false)
        {
            var schema = configuration.GetValue<string>("Schema");
            if (null == schema)
            {
                throw new ArgumentNullException("configuration.schema");
            }
            return schema switch
            {
                "OIDC" => backend ? configuration.Get<OIDCAuthConfigBackend>() : configuration.Get<OIDCAuthConfig>(),
                "Basic" => backend ? configuration.Get<BasicAuthConfigBackend>() : configuration.Get<BasicAuthConfig>(),
                _ => throw new ArgumentException($"Unkown schema {schema}"),
            };;
        }
    }
}
