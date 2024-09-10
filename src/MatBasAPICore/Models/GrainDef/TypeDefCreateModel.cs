using System.ComponentModel.DataAnnotations;

namespace MarBasAPICore.Models.GrainDef
{
    public class TypeDefCreateModel : ITypeDefCreateModel
    {
        private ISet<Guid>? _mixins;
        private string? _name;

        [Required]
        public string Name { get => _name!; set => _name = value; }
        public Guid? ParentId { get; set; }
        public string? Impl { get; set; }
        public IEnumerable<Guid>? MixInIds { get => _mixins; set => _mixins = value?.ToHashSet(); }
    }
}
