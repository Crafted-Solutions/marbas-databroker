using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Access
{
    public interface ISchemaAclEntry : IAclEntry, IUpdateable
    {
        [ReadOnly(true)]
        Guid? SourceGrainId { get; }
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable? SourceGrain { get; }
    }
}
