using System.ComponentModel.DataAnnotations;

namespace MarBasAPICore.Models.Grain
{
    public interface IGrainCreateModel
    {
        [Required]
        Guid ParentId { get; set; }
        Guid? TypeDefId { get; set; }
        [Required]
        string Name { get; set; }
    }
}
