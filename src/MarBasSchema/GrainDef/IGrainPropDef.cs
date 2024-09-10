using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasCommon;
using MarBasSchema.Grain;

namespace MarBasSchema.GrainDef
{
    public interface IGrainPropDef: IGrainBase, IPropDef
    {
        new TraitValueType ValueType { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable? ValueConstraint { get; set; }
     }

    public interface IGrainPropDefLocalized: IGrainPropDef, IGrainLocalized
    {
    }
}
