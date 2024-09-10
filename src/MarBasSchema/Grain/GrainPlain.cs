
namespace MarBasSchema.Grain
{
    public class GrainPlain : IGrain
    {
        public GrainPlain()
        {
        }
        
        public GrainPlain(IGrain other)
        {
            Id = other.Id;
            Name = other.Name;
            ParentId = other.ParentId;
            TypeDefId = other.TypeDefId;
            Path = other.Path;
            CTime = other.CTime;
            MTime = other.MTime;
            Owner = other.Owner;
            Revision = other.Revision;
            SortKey = other.SortKey;
            CustomFlag = other.CustomFlag;
            XAttrs = other.XAttrs;
        }

        public Guid? ParentId { get; set; }

        public string? Path { get; set; }

        public DateTime CTime { get; set; }

        public DateTime MTime { get; set; }

        public string Owner { get; set; } = SchemaDefaults.SystemUserName;

        public int Revision { get; set; } = 1;
        public string? SortKey { get; set; }
        public int CustomFlag { get; set; }
        public string? XAttrs { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; } = "Unnamed";

        public Guid? TypeDefId { get; set; }
    }
}
