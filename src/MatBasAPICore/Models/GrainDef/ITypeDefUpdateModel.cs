using MarBasAPICore.Models.Grain;
using MarBasSchema.GrainDef;

namespace MarBasAPICore.Models.GrainDef
{
    public interface ITypeDefUpdateModel : IGrainUpdateModel<IGrainTypeDef>
    {
        string? Impl { get; set; }
        IEnumerable<Guid>? MixInIds { get; set; }
    }
}
