using MarBasCommon;
using MarBasSchema.Grain;

namespace MarBasSchema.Broker
{
    public interface ICloningBroker
    {
        IGrainBase? CloneGrain(IIdentifiable grain, IIdentifiable? newParent = null, GrainCloneDepth depth = GrainCloneDepth.Self, bool copyAcl = false);
    }
}
