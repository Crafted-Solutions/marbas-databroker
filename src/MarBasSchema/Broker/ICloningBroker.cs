using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface ICloningBroker
    {
        IGrainBase? CloneGrain(IIdentifiable grain, IIdentifiable? newParent = null, GrainCloneDepth depth = GrainCloneDepth.Self, bool copyAcl = false);
        IGrainBase? CreateGrainWithTypeDefaults(string name, IIdentifiable parent, IIdentifiable? typedef);
    }
}
