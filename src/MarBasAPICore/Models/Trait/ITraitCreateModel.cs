using System.ComponentModel.DataAnnotations;
using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasAPICore.Models.Trait
{
    public interface ITraitCreateModel
    {
        [Required]
        Guid GrainId { get; }
        [Required]
        Guid PropDefId { get; }
        string? Culture { get; }
        int? Ord { get; }
        int? Revision { get; }
        TraitValueType? ValueType { get; }
        object? Value { get; }
    }
}
