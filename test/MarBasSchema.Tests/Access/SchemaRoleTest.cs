using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasSchema.Tests.Access
{
    [TestClass]
    public class SchemaRoleTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class RoleMock() : SchemaRole(Guid.NewGuid(), "DummyRole", RoleEntitlement.Full)
        {
        }

        [TestMethod]
        public void Everyone_role_having_correct_properties()
        {
            var role = SchemaRole.Everyone;
            role.Id.Should().Be(SchemaDefaults.EveryoneRoleID);
            role.Name.Should().Be(SchemaDefaults.EveryoneRoleName);
            role.Entitlement.Should().Be(RoleEntitlement.None);
        }

        [TestMethod]
        public void Superuser_role_having_correct_properties()
        {
            var role = SchemaRole.Superuser;
            role.Id.Should().Be(SchemaDefaults.SuperuserRoleID);
            role.Name.Should().Be(SchemaDefaults.SuperuserRoleName);
            role.Entitlement.Should().Be(RoleEntitlement.Full);
        }

        [TestMethod]
        public void CTor_setting_defaults()
        {
            var role = new SchemaRole();

            role.Name.Should().Be($"Role{role.Id:D}");
            role.Entitlement.Should().Be(RoleEntitlement.None);
        }

        [TestMethod]
        public void CTor_setting_Name()
        {
            var name = "TestRole";
            var role = new SchemaRole(name);

            role.Name.Should().Be(name);
            role.Entitlement.Should().Be(RoleEntitlement.None);
        }

        [TestMethod]
        public void CTor_setting_Name_and_Entitlement()
        {
            var name = "TestRole";
            var entitlement = RoleEntitlement.ModifySystemSettings | RoleEntitlement.ImportSchema;
            var role = new SchemaRole(name, entitlement);

            role.Name.Should().Be(name);
            role.Entitlement.Should().Be(entitlement);
        }

        [TestMethod]
        public void CTor_copying_ISchemaRole()
        {
            var source = new RoleMock();
            var role = new SchemaRole(source);

            role.Should().BeEquivalentTo<ISchemaRole>(source);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Name_changing_Name(bool acceptAllChanges)
        {
            var name = "TestRole";
            var role = new SchemaRole(name);
            role.FieldTracker.AcceptAllChanges = acceptAllChanges;

            role.Name.Should().Be(name);

            role.Name = name;
            UpdateableTrackerTest.AssertFieldUpdates<ISchemaRole>(role, acceptAllChanges, nameof(INamed.Name));

            role.Name = "RenamedRole";
            role.Name.Should().NotBe(name);
            role.GetDirtyFields<ISchemaRole>().Should().Satisfy(property => nameof(INamed.Name) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Entitlement_changing_Entitlement(bool acceptAllChanges)
        {
            var entitlement = RoleEntitlement.ImportSchema | RoleEntitlement.ExportSchema;
            var role = new SchemaRole("TestRole", entitlement);
            role.FieldTracker.AcceptAllChanges = acceptAllChanges;

            role.Entitlement.Should().Be(entitlement);

            role.Entitlement = entitlement;
            UpdateableTrackerTest.AssertFieldUpdates<ISchemaRole>(role, acceptAllChanges, nameof(ISchemaRole.Entitlement));

            role.Entitlement = RoleEntitlement.None;
            role.Entitlement.Should().NotBe(entitlement);
            role.GetDirtyFields<ISchemaRole>().Should().Satisfy(property => nameof(ISchemaRole.Entitlement) == property);
        }
    }
}
