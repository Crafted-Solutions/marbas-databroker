using AwesomeAssertions;
using CraftedSolutions.MarBasCommon.Json;
using System.Text;
using System.Text.Json;

namespace CraftedSolutions.MarBasCommon.Tests.Json
{
    [TestClass]
    public class Base64JsonConverterTest
    {
        internal class BytesWrapper
        {
            public byte[] Data { get; set; } = [];
        }

        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new Base64JsonConverter() }
        };

        [TestMethod]
        public void Read_deserializes_base64_to_byte_array()
        {
            var wrapper = JsonSerializer.Deserialize<BytesWrapper>("""{"data": "dGVzdCBzdHJpbmc="}""", _serializerOptions);
            wrapper.Should().NotBeNull();
            wrapper.Data.Should().NotBeNull().And.BeEquivalentTo(Encoding.UTF8.GetBytes("test string"));
        }

        [TestMethod]
        public void Write_serializes_byte_array_to_base64()
        {
            var wrapper = new BytesWrapper()
            {
                Data = Encoding.UTF8.GetBytes("test string")
            };
            var json = JsonSerializer.Serialize(wrapper, _serializerOptions);
            json.Should().NotBeNull().And.Be("""{"data":"dGVzdCBzdHJpbmc="}""");
        }
    }
}
