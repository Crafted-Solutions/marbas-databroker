using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarBasCommon.Json
{
    public sealed class InterfaceJsonConverter<TIface, TModel> : JsonConverter<TIface> where TModel : class, TIface
    {
        public override TIface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TModel>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, TIface value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, typeof(TModel), options);
        }
    }
}
