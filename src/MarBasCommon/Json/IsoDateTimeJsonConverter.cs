using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarBasCommon.Json
{
    public sealed class IsoDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            return string.IsNullOrEmpty(s) ? DateTime.UtcNow : DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)); //.ToString("yyyy-MM-ddTHH:mm:ss"));
        }
    }
}
