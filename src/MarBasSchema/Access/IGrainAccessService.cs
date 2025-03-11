using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Access
{
    public interface IGrainAccessService
    {
        bool VerfifyAccess(IEnumerable<IIdentifiable> grains, GrainAccessFlag desiredAccess);
    }
}
