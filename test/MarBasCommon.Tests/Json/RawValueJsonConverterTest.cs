using AwesomeAssertions;
using CraftedSolutions.MarBasCommon.Job;
using CraftedSolutions.MarBasCommon.Json;
using System.Text.Json;

namespace CraftedSolutions.MarBasCommon.Tests.Json
{
    [TestClass]
    public class RawValueJsonConverterTest
    {
        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new RawValueJsonConverter<BackgroundJobFlags>() }
        };

        [TestMethod]
        public void Read_deserializes_raw_enum()
        {
            using var job = JsonSerializer.Deserialize<BackgroundJob>("""{"name": "test", "owner": "test", "flags": 3}""", _serializerOptions);
            job.Should().NotBeNull();
            job.Flags.Should().Be(BackgroundJobFlags.Pausable | BackgroundJobFlags.Critical);
        }

        [TestMethod]
        public void Write_serializes_raw_enum()
        {
            using var job = new BackgroundJob("test", "test", BackgroundJobFlags.Pausable | BackgroundJobFlags.Critical);
            var json = JsonSerializer.Serialize(job, _serializerOptions);
            json.Should().NotBeNull().And.Contain("\"flags\":3");
        }
    }
}
