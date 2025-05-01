using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Broker;
using System.ComponentModel.DataAnnotations;

namespace CraftedSolutions.MarBasAPICore.Models.Trait
{
    public interface ITraitLookupModel
    {
        [Required]
        Guid PropDefId { get; }
        string? Culture { get; }
        int? Revision { get; }
        TraitValueType? ValueType { get; }
        object? Value { get; }
        public IEnumerable<ListSortOption<GrainSortField>>? SortOptions { get; set; }
    }
}
