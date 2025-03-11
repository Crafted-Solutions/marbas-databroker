using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public interface IGrainLocalized : IGrainExtended, ILocalized
    {
        string? Label { get; set; }
    }
}
