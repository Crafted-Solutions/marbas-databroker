using System.Text.Json.Serialization;
using MarBasCommon.Json;

namespace MarBasSchema.Access
{
    public interface IAclSubject
    {
        [JsonConverter(typeof(RawValueJsonConverter<GrainAccessFlag>))]
        GrainAccessFlag Permissions { get; }
    }
}
