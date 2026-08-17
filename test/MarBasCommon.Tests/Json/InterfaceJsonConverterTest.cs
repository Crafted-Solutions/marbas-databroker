using AwesomeAssertions;
using CraftedSolutions.MarBasCommon.Json;
using System.Text.Json;

namespace CraftedSolutions.MarBasCommon.Tests.Json
{
    [TestClass]
    public class InterfaceJsonConverterTest
    {
        internal class Ident : IIdentifiable
        {
            public Guid Id { get; set; }
        }
        internal class IdentifiableWrapper
        {
            public IIdentifiable Ident { get; set; } = new Ident();
        }

        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new InterfaceJsonConverter<IIdentifiable, Ident>() }
        };

        [TestMethod]
        public void Read_deserializing_IFace_to_Model()
        {
            var wrapper = JsonSerializer.Deserialize<IdentifiableWrapper>("""{"ident": {"id": "51D66673-C633-42B4-987F-2B1351F0D724"}}""", _serializerOptions);
            wrapper.Should().NotBeNull();
            wrapper.Ident.Should().NotBeNull();
            wrapper.Ident.Id.Should().Be(Guid.Parse("51D66673-C633-42B4-987F-2B1351F0D724"));
        }

        [TestMethod]
        public void Write_serializing_Model_to_IFace()
        {
            var wrapper = new IdentifiableWrapper()
            {
                Ident = new Ident()
                {
                    Id = Guid.Parse("B3C5C4BA-BB55-42A9-9DDD-9A04E151BCAE")
                }
            };
            var json = JsonSerializer.Serialize(wrapper, _serializerOptions);
            json.Should().NotBeNull().And.Be("""{"ident":{"id":"b3c5c4ba-bb55-42a9-9ddd-9a04e151bcae"}}""");
        }
    }
}
