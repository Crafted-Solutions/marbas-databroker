using System.Text.Json;
using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasCommon.Json
{
    public sealed class RawValueJsonConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader); // Ignore the incoming options
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value); // Ignore the incoming options
        }
    }
}
