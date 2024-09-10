using MarBasCommon;

namespace MarBasSchema.Grain
{
    public interface IGrainLocalized: IGrainExtended, ILocalized
    {
        string? Label {get; set;}
    }
}
