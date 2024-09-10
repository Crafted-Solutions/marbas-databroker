using System.ComponentModel;

namespace MarBasSchema.GrainDef
{
    public interface ITypeDef
    {
        string? Impl { get; set; }
        [ReadOnly(true)]
        IEnumerable<Guid> MixInIds { get; }
    }
}
