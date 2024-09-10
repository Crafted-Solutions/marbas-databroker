using MarBasSchema.Access;

namespace MarBasSchema.Grain
{
    public interface IGrainExtended: IGrainBase, IAclSubject
    {
        string? TypeXAttrs { get; }
        int ChildCount { get; }
    }
}
