using System.ComponentModel.DataAnnotations;

namespace MarBasAPICore.Models.GrainDef
{
    public interface ITypeDefCreateModel
    {
        [Required]
        string Name { get; set; }
        Guid? ParentId { get; set; }
        string? Impl { get; set; }
        IEnumerable<Guid>? MixInIds { get; set; }
    }
}
