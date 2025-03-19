using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public interface IGrainBase : IGrain, ITypeConstraint, IUpdateable
    {
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable? Parent { get; set; }
    }
}
