using System.Security.Principal;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IBrokerContext
    {
        IPrincipal User { get; }
        IEnumerable<string> UserRoles { get; }
    }
}
