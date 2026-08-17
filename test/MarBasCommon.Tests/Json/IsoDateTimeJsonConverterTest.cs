using AwesomeAssertions;
using CraftedSolutions.MarBasCommon.Json;
using System.Text.Json;

namespace CraftedSolutions.MarBasCommon.Tests.Json
{
    [TestClass]
    public class IsoDateTimeJsonConverterTest
    {
        internal class DateWrapper
        {
            public DateTime Date { get; set; } = new DateTime(2001, 1, 31, 1, 15, 15, DateTimeKind.Utc);
        };

        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new IsoDateTimeJsonConverter() }
        };

        [TestMethod]
        public void Read_deserializing_ISO_string_to_DateTime()
        {
            var wrapper = JsonSerializer.Deserialize<DateWrapper>("""{"date": "2026-08-16T17:14:16.435Z"}""", _serializerOptions);
            wrapper.Should().NotBeNull();
            wrapper.Date.Should().Be(new DateTime(2026, 8, 16, 17, 14, 16, 435, DateTimeKind.Utc));
        }
        [TestMethod]
        public void Read_deserializing_null_string_to_DateTime_Now()
        {
            var now = DateTime.UtcNow;
            var wrapper = JsonSerializer.Deserialize<DateWrapper>("""{"date": null}""", _serializerOptions);
            wrapper.Should().NotBeNull();
            wrapper.Date.Should()
                .HaveYear(now.Year).And
                .HaveMonth(now.Month).And
                .HaveDay(now.Day);
        }

        [TestMethod]
        public void Write_serializing_DateTime_to_ISO_string()
        {
            var wrapper = new DateWrapper();
            var json = JsonSerializer.Serialize(wrapper, _serializerOptions);
            json.Should().NotBeNull().And.Be("""{"date":"2001-01-31T01:15:15.0000000Z"}""");
        }
    }
}
