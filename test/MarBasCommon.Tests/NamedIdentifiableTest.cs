using AwesomeAssertions;

namespace CraftedSolutions.MarBasCommon.Tests
{
    [TestClass]
    public class NamedIdentifiableTest
    {
        [TestMethod]
        public void CTor_copying_INamedIdentifiable()
        {
            var first = new NamedIdentifiable(Guid.NewGuid(), "test");
            var second = new NamedIdentifiable(first);
            second.Id.Should().Be(first.Id);
            second.Name.Should().Be(first.Name);
        }
        [TestMethod]
        public void CTor_copying_IIdentifiable()
        {
            var first = new Identifiable();
            var second = new NamedIdentifiable(first);
            second.Id.Should().Be(first);
            second.Name.Should().BeNull();
        }
    }
}
