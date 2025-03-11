using System.ComponentModel;

namespace CraftedSolutions.MarBasSchema.GrainDef
{
    public interface ITypeDef
    {
        string? Impl { get; set; }
        [ReadOnly(true)]
        IEnumerable<Guid> MixInIds { get; }
    }
}
