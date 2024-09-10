using MarBasCommon;

namespace MarBasSchema.Access
{
    public interface IGrainAccessService
    {
        bool VerfifyAccess(IEnumerable<IIdentifiable> grains, GrainAccessFlag desiredAccess);
    }
}
