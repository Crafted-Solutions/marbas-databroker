using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Access
{
    public interface IAclEntryRef: IGrainBinding
    {
        Guid RoleId { get; }
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable Role { get; set; }
    }
}
