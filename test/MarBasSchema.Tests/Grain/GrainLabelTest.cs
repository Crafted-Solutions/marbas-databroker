using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Tests.Grain
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    class GrainLabelMock : IGrainLabel
    {
        Identifiable _grain = new ();
        CultureInfo _culture = CultureInfo.InvariantCulture;

        public string? Label { get => "Test 42"; set => throw new NotImplementedException(); }
        public IIdentifiable Grain { get => _grain; set => throw new NotImplementedException(); }

        public Guid GrainId => _grain.Id;

        public CultureInfo CultureInfo => _culture;

        public string Culture => _culture.Name;
    }

    [TestClass]
    public class GrainLabelTest
    {
        [TestMethod]
        public void CTor_setting_Label_and_Grain()
        {
            var label = "Test";
            var grain = new Identifiable();
            var grainLabel = new GrainLabel(label, grain);

            grainLabel.Label.Should().Be(label);
            grainLabel.Grain.Should().Be(grain);
            grainLabel.GrainId.Should().Be(grain.Id);
            grainLabel.CultureInfo.Should().Be(SchemaDefaults.Culture);
            grainLabel.Culture.Should().Be(SchemaDefaults.Culture.Name);
        }

        [TestMethod]
        public void CTor_setting_Label_Grain_and_Culture()
        {
            var label = "Test";
            var grain = new Identifiable();
            var culture = CultureInfo.GetCultureInfo("de");
            var grainLabel = new GrainLabel(label, grain, culture);

            grainLabel.Label.Should().Be(label);
            grainLabel.Grain.Should().Be(grain);
            grainLabel.GrainId.Should().Be(grain.Id);
            grainLabel.CultureInfo.Should().Be(culture);
            grainLabel.Culture.Should().Be(culture.Name);
        }

        [TestMethod]
        public void CTor_copying_IGrainLabel_which_is_IUpdateable()
        {
            var source = new GrainLabel("Test", new Identifiable()) { CultureInfo = CultureInfo.GetCultureInfo("en-US") };
            var grainLabel = new GrainLabel(source);

            grainLabel.Should().BeEquivalentTo(source);
        }

        [TestMethod]
        public void CTor_copying_IGrainLabel_which_is_not_IUpdateable()
        {
            var source = new GrainLabelMock();
            var grainLabel = new GrainLabel(source);

            grainLabel.Should().BeEquivalentTo<IGrainLabel>(source);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Label_changing_Label(bool acceptAllChanges)
        {
            var label = "Test";
            var grainLabel = new GrainLabel(label, new Identifiable());
            grainLabel.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grainLabel.Label.Should().Be(label);

            grainLabel.Label = "Test";
            grainLabel.Label.Should().Be(label);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainLabel>(grainLabel, acceptAllChanges, nameof(ILabeled.Label));

            grainLabel.Label = "Anything";
            grainLabel.Label.Should().NotBe(label);
            grainLabel.GetDirtyFields<IGrainLabel>().Should().Satisfy(property => nameof(ILabeled.Label) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Grain_changing_Grain_and_GrainId(bool acceptAllChanges)
        {
            var grain = new Identifiable();
            var grainLabel = new GrainLabel("Test", grain);
            grainLabel.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grainLabel.Grain.Should().Be(grain);

            grainLabel.Grain = grain;
            grainLabel.Grain.Should().Be(grain);
            grainLabel.GrainId.Should().Be(grain.Id);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainLabel>(grainLabel, acceptAllChanges, nameof(IGrainBinding.Grain));

            grainLabel.Grain = new Identifiable();
            grainLabel.Grain.Should().NotBe(grain);
            grainLabel.GrainId.Should().NotBe(grain.Id);
            grainLabel.GetDirtyFields<IGrainLabel>().Should().Satisfy(property => nameof(IGrainBinding.Grain) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_CultureInfo_changing_CultureInfo_and_Culture(bool acceptAllChanges)
        {
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var grainLabel = new GrainLabel("Test", new Identifiable(), culture);
            grainLabel.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grainLabel.CultureInfo.Should().Be(culture);

            grainLabel.CultureInfo = culture;
            grainLabel.Culture.Should().Be(culture.Name);
            grainLabel.CultureInfo.Should().Be(culture);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainLabel>(grainLabel, acceptAllChanges, nameof(ILocalized.CultureInfo));

            grainLabel.CultureInfo = CultureInfo.GetCultureInfo("en");
            grainLabel.CultureInfo.Should().NotBe(culture);
            grainLabel.Culture.Should().NotBe(culture.Name);
            grainLabel.GetDirtyFields<IGrainLabel>().Should().Satisfy(property => nameof(ILocalized.CultureInfo) == property);
        }

    }
}
