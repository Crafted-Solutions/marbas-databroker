using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public interface IGrainExtended : IGrainBase, IAclSubject
    {
        string? TypeXAttrs { get; }
        int ChildCount { get; }
    }
}
