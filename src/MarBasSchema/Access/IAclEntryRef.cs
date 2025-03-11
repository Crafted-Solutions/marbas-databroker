using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Access
{
    public interface IAclEntryRef
    {
        Guid RoleId { get; }
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable Role { get; set; }
        Guid GrainId { get; }
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable Grain { get; set; }
    }
}
