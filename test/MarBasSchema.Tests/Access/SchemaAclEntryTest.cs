using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Tests.Access
{
    [TestClass]
    public class SchemaAclEntryTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class SchemaAclEntryMock() : SchemaAclEntry(new GrainPlain(), new SchemaRole())
        {
            public new IIdentifiable? SourceGrain { get => _sourceGrain; set { _sourceGrain = value; } }
        }
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class AclEntryMock : IAclEntry
        {
            public bool Inherit { get; set; }
            public GrainAccessFlag PermissionMask { get; set; }
            public GrainAccessFlag RestrictionMask { get; set; }

            public Guid RoleId => Role.Id;

            public IIdentifiable Role { get; set; } = new Identifiable();
            public IIdentifiable Grain { get; set; } = new Identifiable();

            public Guid GrainId => Grain.Id;
        }

        [TestMethod]
        public void CTor_setting_Role_and_Grain()
        {
            var grain = new GrainPlain();
            var role = new SchemaRole();
            var entry = new SchemaAclEntry(role, grain);

            entry.Grain.Should().Be(grain);
            entry.GrainId.Should().Be(grain.Id);
            entry.Role.Should().Be(role);
            entry.RoleId.Should().Be(role.Id);
            entry.PermissionMask.Should().Be(GrainAccessFlag.Read);
            entry.RestrictionMask.Should().Be(GrainAccessFlag.None);
            entry.Inherit.Should().BeFalse();
            entry.SourceGrainId.Should().BeNull();
        }

        [TestMethod]
        public void CTor_setting_Role_Grain_and_PermissionMask()
        {
            var grain = new GrainPlain();
            var role = new SchemaRole();
            var permissions = GrainAccessFlag.Read | GrainAccessFlag.Write;
            var entry = new SchemaAclEntry(role, grain, permissions);

            entry.Grain.Should().Be(grain);
            entry.GrainId.Should().Be(grain.Id);
            entry.Role.Should().Be(role);
            entry.RoleId.Should().Be(role.Id);
            entry.PermissionMask.Should().Be(permissions);
            entry.RestrictionMask.Should().Be(GrainAccessFlag.None);
            entry.Inherit.Should().BeFalse();
            entry.SourceGrainId.Should().BeNull();
        }

        [TestMethod]
        public void CTor_setting_Role_Grain_PermissionMask_and_RestrictionMask()
        {
            var grain = new GrainPlain();
            var role = new SchemaRole();
            var permissions = GrainAccessFlag.Read | GrainAccessFlag.Write;
            var restrictions = GrainAccessFlag.CreateSubelement;
            var entry = new SchemaAclEntry(role, grain, permissions, restrictions);

            entry.Grain.Should().Be(grain);
            entry.GrainId.Should().Be(grain.Id);
            entry.Role.Should().Be(role);
            entry.RoleId.Should().Be(role.Id);
            entry.PermissionMask.Should().Be(permissions);
            entry.RestrictionMask.Should().Be(restrictions);
            entry.Inherit.Should().BeFalse();
            entry.SourceGrainId.Should().BeNull();
        }

        [TestMethod]
        public void CTor_setting_Role_Grain_PermissionMask_RestrictionMask_and_Inherit()
        {
            var grain = new GrainPlain();
            var role = new SchemaRole();
            var permissions = GrainAccessFlag.Read | GrainAccessFlag.Write;
            var restrictions = GrainAccessFlag.CreateSubelement;
            var entry = new SchemaAclEntry(role, grain, permissions, restrictions, true);

            entry.Grain.Should().Be(grain);
            entry.GrainId.Should().Be(grain.Id);
            entry.Role.Should().Be(role);
            entry.RoleId.Should().Be(role.Id);
            entry.PermissionMask.Should().Be(permissions);
            entry.RestrictionMask.Should().Be(restrictions);
            entry.Inherit.Should().BeTrue();
            entry.SourceGrainId.Should().BeNull();
        }

        [TestMethod]
        public void CTor_copying_ISchemaAclEntry()
        {
            var source = new SchemaAclEntryMock()
            {
                PermissionMask = GrainAccessFlag.Read | GrainAccessFlag.Write,
                RestrictionMask = GrainAccessFlag.CreateSubelement,
                Inherit = true,
                SourceGrain = new GrainPlain()
            };
            var entry = new SchemaAclEntry(source);
            entry.Should().BeEquivalentTo<ISchemaAclEntry>(source);
        }

        [TestMethod]
        public void CTor_copying_IAclEntry()
        {
            var source = new AclEntryMock()
            {
                PermissionMask = GrainAccessFlag.Read | GrainAccessFlag.Write,
                RestrictionMask = GrainAccessFlag.CreateSubelement,
                Inherit = true
            };
            var entry = new SchemaAclEntry(source);
            entry.Should().BeEquivalentTo<IAclEntry>(source);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Role_changing_Role_and_RoleId(bool acceptAllChanges)
        {
            var role = new SchemaRole("Role1");
            var entry = new SchemaAclEntry(role, new GrainPlain());
            entry.FieldTracker.AcceptAllChanges = acceptAllChanges;

            entry.Role.Should().BeSameAs(role);
            entry.RoleId.Should().Be(role.Id);

            entry.Role = role;
            entry.RoleId.Should().Be(role.Id);
            UpdateableTrackerTest.AssertFieldUpdates<ISchemaAclEntry>(entry, acceptAllChanges, nameof(IAclEntryRef.Role));

            entry.Role = new SchemaRole("Role2");
            entry.Role.Should().NotBe(role);
            entry.RoleId.Should().NotBe(role.Id);
            entry.GetDirtyFields<ISchemaAclEntry>().Should().Satisfy(property => nameof(IAclEntryRef.Role) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Grain_changing_Grain_and_GrainId(bool acceptAllChanges)
        {
            var grain = new GrainPlain();
            var entry = new SchemaAclEntry(new SchemaRole(), grain);
            entry.FieldTracker.AcceptAllChanges = acceptAllChanges;

            entry.Grain.Should().BeSameAs(grain);
            entry.GrainId.Should().Be(grain.Id);

            entry.Grain = grain;
            entry.GrainId.Should().Be(grain.Id);
            UpdateableTrackerTest.AssertFieldUpdates<ISchemaAclEntry>(entry, acceptAllChanges, nameof(IGrainBinding.Grain));

            entry.Grain = new GrainPlain() { Id = Guid.NewGuid() };
            entry.Grain.Should().NotBe(grain);
            entry.GrainId.Should().NotBe(grain.Id);
            entry.GetDirtyFields<ISchemaAclEntry>().Should().Satisfy(property => nameof(IGrainBinding.Grain) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Inherit_changing_Inherit(bool acceptAllChanges)
        {
            var entry = new SchemaAclEntry(new SchemaRole(), new GrainPlain());
            entry.FieldTracker.AcceptAllChanges = acceptAllChanges;

            entry.Inherit.Should().BeFalse();

            entry.Inherit = false;
            UpdateableTrackerTest.AssertFieldUpdates<ISchemaAclEntry>(entry, acceptAllChanges, nameof(IAclEntry.Inherit));

            entry.Inherit = true;
            entry.Inherit.Should().BeTrue();
            entry.GetDirtyFields<ISchemaAclEntry>().Should().Satisfy(property => nameof(IAclEntry.Inherit) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_PermissionMask_changing_PermissionMask(bool acceptAllChanges)
        {
            var entry = new SchemaAclEntry(new SchemaRole(), new GrainPlain());
            entry.FieldTracker.AcceptAllChanges = acceptAllChanges;

            entry.PermissionMask.Should().Be(GrainAccessFlag.Read);

            entry.PermissionMask = GrainAccessFlag.Read;
            UpdateableTrackerTest.AssertFieldUpdates<ISchemaAclEntry>(entry, acceptAllChanges, nameof(IAclEntry.PermissionMask));

            entry.PermissionMask = GrainAccessFlag.Full;
            entry.PermissionMask.Should().Be(GrainAccessFlag.Full);
            entry.GetDirtyFields<ISchemaAclEntry>().Should().Satisfy(property => nameof(IAclEntry.PermissionMask) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_RestrictionMask_changing_RestrictionMask(bool acceptAllChanges)
        {
            var entry = new SchemaAclEntry(new SchemaRole(), new GrainPlain());
            entry.FieldTracker.AcceptAllChanges = acceptAllChanges;

            entry.RestrictionMask.Should().Be(GrainAccessFlag.None);

            entry.RestrictionMask = GrainAccessFlag.None;
            UpdateableTrackerTest.AssertFieldUpdates<ISchemaAclEntry>(entry, acceptAllChanges, nameof(IAclEntry.RestrictionMask));

            entry.RestrictionMask = GrainAccessFlag.Delete;
            entry.RestrictionMask.Should().Be(GrainAccessFlag.Delete);
            entry.GetDirtyFields<ISchemaAclEntry>().Should().Satisfy(property => nameof(IAclEntry.RestrictionMask) == property);
        }
    }
}
