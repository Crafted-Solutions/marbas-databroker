using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public interface ITrait : IIdentifiable, IValueTypeConstraint, ITraitRef
    {
        int Ord { get; set; }
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        bool IsNull { get; }
        [ReadOnly(true)]
        object? Value { get; }
    }
}
