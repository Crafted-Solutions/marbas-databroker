using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.GrainTier
{
    public enum GrainFileContentAccess
    {
        None, OnDemand, Immediate
    }

    public interface IGrainFile : IGrainBase, IFile
    {
    }
}
