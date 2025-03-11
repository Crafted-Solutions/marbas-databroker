using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Access
{
    public interface IAsyncGrainAccessService
    {
        Task<bool> VerfifyAccessAsync(IEnumerable<IIdentifiable> grains, GrainAccessFlag desiredAccess, CancellationToken cancellationToken = default);
    }
}
