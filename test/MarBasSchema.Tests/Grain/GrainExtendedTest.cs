using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Tests.Grain
{
    [TestClass]
    public class GrainExtendedTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class GrainExtendedMock: GrainExtended
        {
            public GrainExtendedMock(GrainAccessFlag permissions = GrainAccessFlag.None, string? typeXAttrs = null, int childCount = 0)
            {
                _permissions = permissions;
                _typeXAttrs = typeXAttrs;
                _childCount = childCount;
            }
        }

        [TestMethod]
        public void CTor_setting_defaults_given_no_parameters()
        {
            var grain = new GrainExtended();

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
        }

        [TestMethod]
        public void CTor_setting_Name()
        {
            var name = "Grain42";
            var grain = new GrainExtended(name);

            grain.Name.Should().Be(name);

            grain.TypeXAttrs.Should().BeNull();
            grain.Permissions.Should().Be(GrainAccessFlag.Read);
            grain.ChildCount.Should().Be(0);
        }

        [TestMethod]
        public void CTor_setting_Name_and_Parent()
        {
            var name = "Grain42";
            var parent = new Identifiable();
            var grain = new GrainExtended(name, parent);

            grain.Name.Should().Be(name);
            grain.Parent.Should().Be(parent);
            grain.ParentId.Should().Be(parent.Id);

            grain.TypeXAttrs.Should().BeNull();
            grain.Permissions.Should().Be(GrainAccessFlag.Read);
            grain.ChildCount.Should().Be(0);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("tester")]
        public void CTor_setting_Name_Parent_and_Owner(string ownerName)
        {
            var name = "Grain42";
            var parent = new Identifiable();
            var owner = GrainBaseTest.MakePrincipal(ownerName);
            var grain = new GrainExtended(name, parent, owner);

            grain.Name.Should().Be(name);
            grain.Owner.Should().Be(string.IsNullOrEmpty(ownerName) ? SchemaDefaults.SystemUserName : owner.Identity!.Name);
            grain.Parent.Should().Be(parent);
            grain.ParentId.Should().Be(parent.Id);

            grain.TypeXAttrs.Should().BeNull();
            grain.Permissions.Should().Be(GrainAccessFlag.Read);
            grain.ChildCount.Should().Be(0);
        }

        [TestMethod]
        public void CTor_copying_IGrainExtended()
        {
            var source = new GrainExtendedMock(GrainAccessFlag.Read | GrainAccessFlag.Write, "\"silo\":{}", 42);
            var grain = new GrainExtended(source);

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
            var grain = new GrainExtended(source);

            grain.Id.Should().Be(source.Id);
            grain.Name.Should().Be(source.Name);
            grain.Owner.Should().Be(source.Owner);
            grain.Path.Should().Be(source.Path);
            grain.ParentId.Should().Be(source.ParentId);
            grain.CTime.Should().Be(source.CTime);
            grain.MTime.Should().Be(source.MTime);
            grain.TypeDefId.Should().Be(source.TypeDefId);

            grain.TypeXAttrs.Should().BeNull();
            grain.Permissions.Should().Be(GrainAccessFlag.Read);
            grain.ChildCount.Should().Be(0);
        }
    }
}
