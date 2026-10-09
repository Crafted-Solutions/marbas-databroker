using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using System.Data;
using System.Security.Claims;
using System.Security.Principal;

namespace CraftedSolutions.MarBasSchema.Tests.Grain
{
    [TestClass]
    public class GrainBaseTest
    {
        [TestMethod]
        public void CTor_setting_defaults_given_no_parameters()
        {
            var grain = new GrainBase();

            grain.Name.Should().Be($"Unnamed_{grain.Id:D}");
            grain.Owner.Should().Be(SchemaDefaults.SystemUserName);
            grain.Path.Should().BeNull();
            grain.Parent.Should().BeNull();
            grain.ParentId.Should().BeNull();
            grain.TypeDef.Should().BeNull();
            grain.TypeDefId.Should().BeNull();
            grain.TypeName.Should().BeNull();
            grain.GetDirtyFields<IGrainBase>().Should().BeEmpty();
        }

        [TestMethod]
        public void CTor_setting_Name()
        {
            var name = "Grain42";
            var grain = new GrainBase(name);

            grain.Name.Should().Be(name);
            grain.Owner.Should().Be(SchemaDefaults.SystemUserName);
            grain.Path.Should().BeNull();
            grain.Parent.Should().BeNull();
            grain.ParentId.Should().BeNull();
            grain.TypeDef.Should().BeNull();
            grain.TypeDefId.Should().BeNull();
            grain.TypeName.Should().BeNull();
            grain.GetDirtyFields<IGrainBase>().Should().BeEmpty();
        }

        [TestMethod]
        public void CTor_setting_Name_and_Parent()
        {
            var name = "Grain42";
            var parent = new Identifiable();
            var grain = new GrainBase(name, parent);

            grain.Name.Should().Be(name);
            grain.Owner.Should().Be(SchemaDefaults.SystemUserName);
            grain.Path.Should().BeNull();
            grain.Parent.Should().Be(parent);
            grain.ParentId.Should().Be(parent.Id);
            grain.TypeDef.Should().BeNull();
            grain.TypeDefId.Should().BeNull();
            grain.TypeName.Should().BeNull();
            grain.GetDirtyFields<IGrainBase>().Should().BeEmpty();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("tester")]
        public void CTor_setting_Name_Parent_and_Owner(string ownerName)
        {
            var name = "Grain42";
            var parent = new Identifiable();
            var owner = MakePrincipal(ownerName);
            var grain = new GrainBase(name, parent, owner);

            grain.Name.Should().Be(name);
            grain.Owner.Should().Be(string.IsNullOrEmpty(ownerName) ? SchemaDefaults.SystemUserName : owner.Identity!.Name);
            grain.Path.Should().BeNull();
            grain.Parent.Should().Be(parent);
            grain.ParentId.Should().Be(parent.Id);
            grain.TypeDef.Should().BeNull();
            grain.TypeDefId.Should().BeNull();
            grain.TypeName.Should().BeNull();
            grain.GetDirtyFields<IGrainBase>().Should().BeEmpty();
        }

        [TestMethod]
        public void CTor_copying_IGrainBase()
        {
            var source = new GrainBase("Grain42", new Identifiable(), MakePrincipal("tester"))
            {
                TypeDef = new Identifiable()
            };
            var grain = new GrainBase(source);

            grain.Should().BeEquivalentTo(source);
        }

        [TestMethod]
        public void CTor_copying_IGrainBase_overriding_imcomplete_TypeDef()
        {
            var source = new GrainBase("Grain42", new Identifiable(), MakePrincipal("tester"))
            {
                TypeDef = new Identifiable()
            };
            var typeConstraints = new SimpleTypeConstraint(SchemaDefaults.ElementTypeDefID, "Element");
            var grain = new GrainBase(source, typeConstraints);

            grain.Id.Should().Be(source.Id);
            grain.Name.Should().Be(source.Name);
            grain.Owner.Should().Be(source.Owner);
            grain.Path.Should().Be(source.Path);
            grain.Parent.Should().Be(source.Parent);
            grain.ParentId.Should().Be(source.ParentId);
            grain.CTime.Should().Be(source.CTime);
            grain.MTime.Should().Be(source.MTime);
            grain.TypeDef.Should().Be(typeConstraints.TypeDef);
            grain.TypeDefId.Should().Be(typeConstraints.TypeDefId);
            grain.TypeName.Should().Be(typeConstraints.TypeName);
            grain.FieldTracker.AllChanges.Should().BeEquivalentTo(source.FieldTracker.AllChanges);
        }

        [TestMethod]
        public void CTor_copying_IGrainBase_not_overriding_complete_TypeDef()
        {
            var source = new GrainBase("Grain42", new Identifiable(), MakePrincipal("tester"))
            {
                TypeDef = new NamedIdentifiable(SchemaDefaults.ContainerTypeDefID, "Container")
            };
            var typeConstraints = new SimpleTypeConstraint(SchemaDefaults.ElementTypeDefID, "Element");
            var grain = new GrainBase(source, typeConstraints);


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
            var grain = new GrainBase(source);

            grain.Id.Should().Be(source.Id);
            grain.Name.Should().Be(source.Name);
            grain.Owner.Should().Be(source.Owner);
            grain.Path.Should().Be(source.Path);
            grain.ParentId.Should().Be(source.ParentId);
            grain.CTime.Should().Be(source.CTime);
            grain.MTime.Should().Be(source.MTime);
            grain.TypeDefId.Should().Be(source.TypeDefId);
        }

        [TestMethod]
        public void CTor_copying_IGrain_overriding_imcomplete_TypeDef()
        {
            var source = new GrainPlain()
            {
                Name = "Grain42",
                Path = "marbas/Content/Grain42",
                Owner = "tester",
                TypeDefId = Guid.NewGuid()
            };
            var typeConstraints = new SimpleTypeConstraint(SchemaDefaults.ElementTypeDefID, "Element");
            var grain = new GrainBase(source, typeConstraints);

            grain.Id.Should().Be(source.Id);
            grain.Name.Should().Be(source.Name);
            grain.Owner.Should().Be(source.Owner);
            grain.Path.Should().Be(source.Path);
            grain.ParentId.Should().Be(source.ParentId);
            grain.CTime.Should().Be(source.CTime);
            grain.MTime.Should().Be(source.MTime);
            grain.TypeDef.Should().Be(typeConstraints.TypeDef);
            grain.TypeDefId.Should().Be(typeConstraints.TypeDefId);
            grain.TypeName.Should().Be(typeConstraints.TypeName);
        }

        [TestMethod]
        public void Clone_producing_identical_IGrainBase()
        {
            var source = new GrainBase(new GrainPlain()
            {
                Name = "Grain42",
                Path = "marbas/Content/Grain42",
                Owner = "tester",
                ParentId = Guid.NewGuid(),
                TypeDefId = Guid.NewGuid()
            });
            var grain = source.Clone();

            grain.Should().BeEquivalentTo(source);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Parent_changing_Parent_and_ParentId(bool acceptAllChanges)
        {
            var parent = new Identifiable();
            var grain = new GrainBase();
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.Parent.Should().Be(null);
            grain.ParentId.Should().Be(null);

            grain.Parent = null;
            grain.ParentId.Should().BeNull();
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(IGrainBase.Parent));

            grain.Parent = parent;
            grain.Parent.Should().Be(parent);
            grain.ParentId.Should().Be(parent.Id);
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(IGrainBase.Parent) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_TypeDef_changing_TypeDef_and_TypeDefId(bool acceptAllChanges)
        {
            var typeDef = new Identifiable();
            var grain = new GrainBase()
            {
                TypeDef = typeDef
            };
            grain.GetDirtyFields<IGrainBase>().Clear();
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.TypeDef.Should().NotBeNull();
            grain.TypeDefId.Should().Be(typeDef.Id);
            grain.TypeName.Should().BeNull();

            grain.TypeDef = typeDef;
            grain.TypeDefId.Should().Be(typeDef.Id);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(ITypeConstraint.TypeDef));

            grain.TypeDef = null;
            grain.TypeDef.Should().BeNull();
            grain.TypeDefId.Should().BeNull();
            grain.TypeName.Should().BeNull();
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(ITypeConstraint.TypeDef) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_TypeDef_changing_TypeDef_TypeDefId_and_TypeName(bool acceptAllChanges)
        {
            var typeDef = new NamedIdentifiable(SchemaDefaults.ElementTypeDefID, "Element");
            var grain = new GrainBase();
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.TypeDef.Should().BeNull();
            grain.TypeDefId.Should().BeNull();
            grain.TypeName.Should().BeNull();

            grain.TypeDef = null;
            grain.TypeDefId.Should().BeNull();
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(ITypeConstraint.TypeDef));

            grain.TypeDef = typeDef;
            grain.TypeDef.Should().NotBeNull();
            grain.TypeDefId.Should().Be(typeDef.Id);
            grain.TypeName.Should().Be(typeDef.Name);
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(IGrainBase.TypeDef) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Name_changing_Name_and_Path(bool acceptAllChanges)
        {
            var template = new GrainPlain()
            {
                Name = "Grain42",
                Path = "marbas/Content/Grain42"
            };
            var grain = new GrainBase(template);
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.Name.Should().Be(template.Name);
            grain.Path.Should().Be(template.Path);

            grain.Name = template.Name;
            grain.Name.Should().Be(template.Name);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(INamed.Name));

            grain.Name = "NewName";
            grain.Name.Should().NotBe(template.Name);
            grain.Path.Should().Be($"marbas/Content/{grain.Name}");
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(INamed.Name) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Name_sanitizing_invalid_Name(bool acceptAllChanges)
        {
            var grain = new GrainBase() { Name = "" };
            grain.GetDirtyFields<IGrainBase>().Clear();
            var unnamed = $"Unnamed_{grain.Id:D}";
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.Name.Should().Be(unnamed);
            grain.Path.Should().BeNull();

            grain.Name = unnamed;
            grain.Name.Should().Be(unnamed);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(INamed.Name));

            grain.Name = "/Unsanitized:Name?";
            grain.Name.Should().Be("!Unsanitized!Name!");
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(INamed.Name) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Name_truncating_overlong_Name(bool acceptAllChanges)
        {
            var name = "Grain42";
            var grain = new GrainBase(name);
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.Name.Should().Be(name);

            grain.Name = name;
            grain.Name.Should().Be(name);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(INamed.Name));

            grain.Name = new string('s', 300);
            grain.Name.Should().Be($"{new string('s', 252)}...");
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(INamed.Name) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_CTime_changing_CTime(bool acceptAllChanges)
        {
            var template = new GrainPlain();
            var grain = new GrainBase(template);
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.CTime.Should().Be(template.CTime);

            grain.CTime = template.CTime;
            grain.CTime.Should().Be(template.CTime);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(IGrain.CTime));

            grain.CTime = DateTime.UtcNow;
            grain.CTime.Should().NotBe(template.CTime);
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(IGrain.CTime) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_MTime_changing_MTime(bool acceptAllChanges)
        {
            var template = new GrainPlain();
            var grain = new GrainBase(template);
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.MTime.Should().Be(template.MTime);

            grain.MTime = template.MTime;
            grain.MTime.Should().Be(template.MTime);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(IGrain.MTime));

            grain.MTime = DateTime.UtcNow;
            grain.MTime.Should().NotBe(template.MTime);
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(IGrain.MTime) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Owner_changing_Owner(bool acceptAllChanges)
        {
            var grain = new GrainBase();
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.Owner.Should().Be(SchemaDefaults.SystemUserName);

            grain.Owner = SchemaDefaults.SystemUserName;
            grain.Owner.Should().Be(SchemaDefaults.SystemUserName);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(IGrain.Owner));

            grain.Owner = "tester";
            grain.Owner.Should().NotBe(SchemaDefaults.SystemUserName);
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(IGrain.Owner) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Revision_changing_Revision(bool acceptAllChanges)
        {
            var grain = new GrainBase();
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.Revision.Should().Be(1);

            grain.Revision = 1;
            grain.Revision.Should().Be(1);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(IGrain.Revision));

            grain.Revision = 2;
            grain.Revision.Should().NotBe(1);
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(IGrain.Revision) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_CustomFlag_changing_CustomFlag(bool acceptAllChanges)
        {
            var grain = new GrainBase();
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.CustomFlag.Should().Be(0);

            grain.CustomFlag = 0;
            grain.CustomFlag.Should().Be(0);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(IGrain.CustomFlag));

            grain.CustomFlag = 0x1000;
            grain.CustomFlag.Should().NotBe(0);
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(IGrain.CustomFlag) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_SortKey_changing_SortKey(bool acceptAllChanges)
        {
            var grain = new GrainBase();
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.SortKey.Should().BeNull();

            grain.SortKey = null;
            grain.SortKey.Should().BeNull();
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(IGrain.SortKey));

            grain.SortKey = "100";
            grain.SortKey.Should().NotBeNull();
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(IGrain.SortKey) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_XAttrs_changing_XAttrs(bool acceptAllChanges)
        {
            var grain = new GrainBase();
            grain.FieldTracker.AcceptAllChanges = acceptAllChanges;

            grain.XAttrs.Should().BeNull();

            grain.XAttrs = null;
            grain.XAttrs.Should().BeNull();
            UpdateableTrackerTest.AssertFieldUpdates<IGrainBase>(grain, acceptAllChanges, nameof(IGrain.XAttrs));

            grain.XAttrs = "silo:{}";
            grain.XAttrs.Should().NotBeNull();
            grain.GetDirtyFields<IGrainBase>().Should().Satisfy(property => nameof(IGrain.XAttrs) == property);
        }

        public static IPrincipal MakePrincipal(string name)
        {
            return string.IsNullOrEmpty(name) ? new ClaimsPrincipal() : new GenericPrincipal(new GenericIdentity(name), null);
        }
    }
}
