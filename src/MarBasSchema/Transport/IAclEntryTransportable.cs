using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasSchema.Access;

namespace MarBasSchema.Transport
{
    [JsonDerivedType(typeof(AclEntryTransportable))]
    public interface IAclEntryTransportable: IAclEntry
    {
        [JsonIgnore]
        [IgnoreDataMember]
        new Guid GrainId { get; }
    }
}
