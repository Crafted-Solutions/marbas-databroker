using System.Security.Principal;

namespace MarBasSchema.Broker
{
    public interface IBrokerContext
    {
        IPrincipal User { get; }
        IEnumerable<string> UserRoles { get; }
    }
}
