using AwesomeAssertions;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Tests.Grain
{
    [TestClass]
    public class GrainPlainTest
    {
        [TestMethod]
        public void CTor_copying_IGrain()
        {
            var source = new GrainPlain()
            {
                Name = "Grain42",
                Owner = "Groundhog",
                Revision = 42
            };
            var grain = new GrainPlain(source);

            grain.Should().BeEquivalentTo(source);
        }
    }
}
