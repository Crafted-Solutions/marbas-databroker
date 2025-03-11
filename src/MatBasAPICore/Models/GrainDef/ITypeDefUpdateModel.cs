using CraftedSolutions.MarBasAPICore.Models.Grain;
using CraftedSolutions.MarBasSchema.GrainDef;

namespace CraftedSolutions.MarBasAPICore.Models.GrainDef
{
    public interface ITypeDefUpdateModel : IGrainUpdateModel<IGrainTypeDef>
    {
        string? Impl { get; set; }
        IEnumerable<Guid>? MixInIds { get; set; }
    }
}
