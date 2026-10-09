using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasSchema.GrainDef
{
    public interface IGrainPropDef : IGrainBase, IPropDef
    {
        new TraitValueType ValueType { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable? ValueConstraint { get; set; }
    }

    public interface IGrainPropDefLocalized : IGrainPropDef, IGrainLocalized
    {
    }
}
