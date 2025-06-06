namespace CraftedSolutions.MarBasAPICore.Auth
{
    public interface IBasicAuthConfig: IAuthConfig
    {
    }

    public class BasicAuthConfig: AuthConfig, IBasicAuthConfig
    {
    }
}
