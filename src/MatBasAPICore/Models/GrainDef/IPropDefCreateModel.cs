using System.ComponentModel.DataAnnotations;

namespace MarBasAPICore.Models.GrainDef
{
    public interface IPropDefCreateModel
    {
        [Required]
        string Name { get; set; }
        [Required]
        Guid TypeContainerId { get; set; }
        string? ValueType { get; set; }
        int? CardinalityMin { get; set; }
        int? CardinalityMax { get; set; }
    }
}
