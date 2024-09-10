using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasCommon;

namespace MarBasSchema.Grain
{
    public interface IGrainBase: IGrain, ITypeConstraint, IUpdateable
    {
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable? Parent { get; set; }
    }
}
