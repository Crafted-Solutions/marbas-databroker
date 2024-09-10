using MarBasSchema.Grain;

namespace MarBasSchema.GrainTier
{
    public enum GrainFileContentAccess
    {
        None, OnDemand, Immediate
    }

    public interface IGrainFile : IGrainBase, IFile
    {
    }
}
