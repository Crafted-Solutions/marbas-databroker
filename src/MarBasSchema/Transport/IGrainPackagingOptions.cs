using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasSchema.Transport
{
    public enum ReferenceDepth
    {
        None, Immediate, Indefinite
    }

    [JsonDerivedType(typeof(GrainPackagingOptions))]
    public interface IGrainPackagingOptions: ICloneable
    {
        int Priority { get; set; }
        ReferenceDepth LinksTraversal { get; set; }
        ReferenceDepth TypeDefTraversal { get; set; }
        ReferenceDepth ParentTraversal { get; set; }
        ReferenceDepth ChildrenTraversal { get; set; }

        new IGrainPackagingOptions Clone();
    }
}
