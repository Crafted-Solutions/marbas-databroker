using Microsoft.Extensions.Configuration;

namespace CraftedSolutions.MarBasCommon
{
    public interface IConfigurable
    {
        IConfiguration Configuration { get; }
    }
}
