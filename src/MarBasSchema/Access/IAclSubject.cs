using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon.Json;

namespace CraftedSolutions.MarBasSchema.Access
{
    public interface IAclSubject
    {
        [JsonConverter(typeof(RawValueJsonConverter<GrainAccessFlag>))]
        GrainAccessFlag Permissions { get; }
    }
}
