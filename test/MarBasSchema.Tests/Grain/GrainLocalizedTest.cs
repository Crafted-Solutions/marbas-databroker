using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Grain;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Tests.Grain
{
    [TestClass]
    public class GrainLocalizedTest
    {

        [TestMethod]
        public void CTor_setting_defaults_given_no_parameters()
        {
            var grain = new GrainLocalized();

            grain.Name.Should().Be($"Unnamed_{grain.Id:D}");
            grain.Owner.Should().Be(SchemaDefaults.SystemUserName);
            grain.Path.Should().BeNull();
            grain.Parent.Should().BeNull();
            grain.ParentId.Should().BeNull();
            grain.TypeDef.Should().BeNull();
            grain.TypeDefId.Should().BeNull();
            grain.TypeName.Should().BeNull();
            grain.GetDirtyFields<IGrainBase>().Should().BeEmpty();
            grain.TypeXAttrs.Should().BeNull();
            grain.Permissions.Should().Be(GrainAccessFlag.Read);
            grain.ChildCount.Should().Be(0);

            grain.Label.Should().Be(grain.Name);
            grain.CultureInfo.Should().Be(SchemaDefaults.Culture);
            grain.Culture.Should().Be(SchemaDefaults.Culture.Name);
        }

        [TestMethod]
        public void CTor_setting_Name_and_Label()
        {
            var name = "Grain42";
            var grain = new GrainLocalized(name);

            grain.Name.Should().Be(name);

            grain.Label.Should().Be(name);
            grain.CultureInfo.Should().Be(SchemaDefaults.Culture);
            grain.Culture.Should().Be(SchemaDefaults.Culture.Name);
        }

        [TestMethod]
        public void CTor_setting_Name_Label_and_Parent()
        {
            var name = "Grain42";
            var parent = new Identifiable();
            var grain = new GrainLocalized(name, parent);

            grain.Name.Should().Be(name);
            grain.Parent.Should().Be(parent);
            grain.ParentId.Should().Be(parent.Id);

            grain.Label.Should().Be(name);
            grain.CultureInfo.Should().Be(SchemaDefaults.Culture);
            grain.Culture.Should().Be(SchemaDefaults.Culture.Name);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("tester")]
        public void CTor_setting_Name_Label_Parent_and_Owner(string ownerName)
        {
            var name = "Grain42";
            var parent = new Identifiable();
            var owner = GrainBaseTest.MakePrincipal(ownerName);
            var grain = new GrainLocalized(name, parent, owner);

            grain.Name.Should().Be(name);
            grain.Owner.Should().Be(string.IsNullOrEmpty(ownerName) ? SchemaDefaults.SystemUserName : owner.Identity!.Name);
            grain.Parent.Should().Be(parent);
            grain.ParentId.Should().Be(parent.Id);

            grain.Label.Should().Be(name);
            grain.CultureInfo.Should().Be(SchemaDefaults.Culture);
            grain.Culture.Should().Be(SchemaDefaults.Culture.Name);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("en-US")]
        public void CTor_copying_IGrainLocalized(string lang)
        {
            var source = new GrainLocalized("Test", new Identifiable(), GrainBaseTest.MakePrincipal("tester"),
                string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang))
            {
                Label = "Test Label"
            };
            var grain = new GrainLocalized(source);

            grain.Should().BeEquivalentTo(source);
        }

        [TestMethod]
        public void CTor_copying_IGrain()
        {
            var source = new GrainPlain()
            {
                Name = "Grain42",
                Path = "marbas/Content/Grain42",
                Owner = "tester",
                ParentId = Guid.NewGuid(),
                TypeDefId = Guid.NewGuid()
            };
            var grain = new GrainLocalized(source);

            grain.Id.Should().Be(source.Id);
            grain.Name.Should().Be(source.Name);
            grain.Owner.Should().Be(source.Owner);
            grain.Path.Should().Be(source.Path);
            grain.ParentId.Should().Be(source.ParentId);
            grain.CTime.Should().Be(source.CTime);
            grain.MTime.Should().Be(source.MTime);
            grain.TypeDefId.Should().Be(source.TypeDefId);

            grain.Label.Should().Be(source.Name);
            grain.CultureInfo.Should().Be(SchemaDefaults.Culture);
            grain.Culture.Should().Be(SchemaDefaults.Culture.Name);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Label_changing_Label(bool acceptAllChanges)
        {
            var grain = new GrainLocalized();
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.Label.Should().Be(grain.Name);

            grain.Label = null;
            grain.Label.Should().Be(grain.Name);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainLocalized>(grain, acceptAllChanges, nameof(ILabeled.Label));

            grain.Label = "Label 42";
            grain.Label.Should().NotBe(grain.Name);
            grain.GetDirtyFields<IGrainLocalized>().Should().Satisfy(property => nameof(ILabeled.Label) == property);
        }
    }
}
