using CraftedSolutions.MarBasSchema.Broker;

namespace CraftedSolutions.MarBasAPICore.Models.Grain
{
    public sealed class GrainCloneModel
    {
        public Guid? NewParentId { get; set; }
        public GrainCloneDepth? Depth { get; set; }
        public bool? CopyACL { get; set; }
    }
}
