namespace CraftedSolutions.MarBasAPICore.Models.Grain
{
    public sealed class GrainCreateModel : IGrainCreateModel
    {
        private Guid _parentId;
        private Guid? _typeDefId;
        private string? _name;

        public Guid ParentId { get => _parentId; set => _parentId = value; }
        public Guid? TypeDefId { get => _typeDefId; set => _typeDefId = value; }
        public string Name { get => _name!; set => _name = value; }
        public bool CopyTypeDefaults { get; set; } = true;
    }
}
