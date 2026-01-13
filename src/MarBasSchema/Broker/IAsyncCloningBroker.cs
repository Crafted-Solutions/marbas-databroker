using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IAsyncCloningBroker
    {
        Task<IGrainBase?> CloneGrainAsync(IIdentifiable grain, IIdentifiable? newParent = null, GrainCloneDepth depth = GrainCloneDepth.Self, bool copyAcl = false, CancellationToken cancellationToken = default);
        Task<IGrainBase?> CreateGrainWithTypeDefaultsAsync(string name, IIdentifiable parent, IIdentifiable? typedef, CancellationToken cancellationToken = default);
    }
}
