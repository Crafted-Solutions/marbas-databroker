using AwesomeAssertions;

namespace CraftedSolutions.MarBasCommon.Tests
{
    [TestClass]
    public class IdentifiableTest
    {
        [TestMethod]
        public void CTor_copying_IIdentifiable()
        {
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
            IIdentifiable first = new Identifiable();
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
            var second = new Identifiable(first);
            second.Id.Should().Be(first.Id);
        }

        [TestMethod]
        public void Identifiable_cast_operator_converting_Guid()
        {
            var id = Guid.NewGuid();
            Identifiable identifiable = id;
            identifiable.Should().NotBeNull();
            identifiable.Id.Should().Be(id);
        }

        [TestMethod]
        public void Guid_cast_operator_converting_Identifiable()
        {
            var identifiable = new Identifiable();
            Guid id = identifiable;
            id.Should().Be(identifiable.Id);
        }
    }
}
