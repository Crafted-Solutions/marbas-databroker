namespace CraftedSolutions.MarBasAPICore.Models.GrainDef
{
    public class PropDefCreateModel : IPropDefCreateModel
    {
        private string? _name;

        public string Name { get => _name!; set => _name = value; }
        public Guid TypeContainerId { get; set; }
        public string? ValueType { get; set; }
        public int? CardinalityMin { get; set; }
        public int? CardinalityMax { get; set; }
    }
}
