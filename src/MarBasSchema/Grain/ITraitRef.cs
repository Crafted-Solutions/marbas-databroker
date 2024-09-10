using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasCommon;

namespace MarBasSchema.Grain
{
    public interface ITraitRef: ILocalizable
    {
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable Grain { get; set; }
        Guid GrainId { get; }
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable PropDef { get; set; }
        Guid PropDefId { get; }
        int Revision { get; set; }
    }
}
