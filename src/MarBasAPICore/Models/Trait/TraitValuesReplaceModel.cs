using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasAPICore.Models.Trait
{
    public sealed class TraitValuesReplaceModel
    {
        public Guid GrainId { get; set; }
        public Guid PropDefId { get; set; }
        public TraitValueType? ValueType { get; set; }
        public string? Culture { get; set; }
        public int Revision { get; set; } = 1;
        public IEnumerable<object?> Values { get; set; } = new List<object?>();
    }
}
