using MarBasSchema.IO;

namespace MarBasSchema.GrainTier
{
    public interface IFile
    {
        string MimeType { get; set; }
        long Size { get; }
        IStreamableContent? Content { get; set; }
    }
}
