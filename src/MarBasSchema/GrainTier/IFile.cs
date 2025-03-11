using CraftedSolutions.MarBasSchema.IO;

namespace CraftedSolutions.MarBasSchema.GrainTier
{
    public interface IFile
    {
        string MimeType { get; set; }
        long Size { get; }
        IStreamableContent? Content { get; set; }
    }
}
