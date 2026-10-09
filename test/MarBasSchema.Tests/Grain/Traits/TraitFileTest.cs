using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain.Traits;

namespace CraftedSolutions.MarBasSchema.Tests.Grain.Traits
{
    [TestClass]
    public class TraitFileTest
    {
        [TestMethod]
        public void CTor_throwing_InvalidCastException_given_incompatible_PropDef()
        {
            var initCall = () => new TraitFile(new Identifiable(), new SimpleValueTypeContraint(new Identifiable()));

            initCall.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void CTor_setting_PropDef_and_PropDefId_keeping_ValueType()
        {
            var propDef = new Identifiable();
            var trait = new TraitFile(new Identifiable(), propDef);

            trait.PropDefId.Should().Be(propDef.Id);
            trait.ValueType.Should().Be(TraitValueType.File);
        }

        [TestMethod]
        public void CTor_copying_TraitFile()
        {
            var value = new Identifiable();
            var source = new TraitFile(new Identifiable(), new Identifiable(), value);
            var file = new TraitFile(source);

            file.Value.Should().Be(value.Id);
        }
    }
}
