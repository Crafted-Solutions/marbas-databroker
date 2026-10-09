using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.GrainDef;
using CraftedSolutions.MarBasSchema.Tests.Grain;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Tests.GrainDef
{
    [TestClass]
    public class GrainPropDefTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class PropDefMock : GrainPlain, IPropDef
        {
            public Guid? ValueConstraintId { get; set; } = new Identifiable();
            public string? ConstraintParams { get; set; } = "use=Something";
            public int CardinalityMin { get; set; } = 42;
            public int CardinalityMax { get; set; } = -1;
            public bool Versionable { get; set; } = true;
            public bool Localizable { get; set; }

            public TraitValueType ValueType => TraitValueType.DateTime;
        }

        [TestMethod]
        public void CTor_setting_defaults_given_no_parameters()
        {
            var reference = new GrainLocalized();
            var propDef = new GrainPropDef();

            propDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(propDef);
        }

        [TestMethod]
        public void CTor_setting_Name()
        {
            var name = "Property42";
            var reference = new GrainLocalized(name);
            var propDef = new GrainPropDef(name);

            propDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(propDef);
        }

        [TestMethod]
        public void CTor_setting_Name_and_Parent()
        {
            var name = "Property42";
            var parent = new Identifiable();
            var reference = new GrainLocalized(name, parent);
            var propDef = new GrainPropDef(name, parent);

            propDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(propDef);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("tester")]
        public void CTor_setting_Name_Parent_and_Owner(string ownerName)
        {
            var name = "Property42";
            var parent = new Identifiable();
            var owner = GrainBaseTest.MakePrincipal(ownerName);
            var reference = new GrainLocalized(name, parent, owner);
            var propDef = new GrainPropDef(name, parent, owner);

            propDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(propDef);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("tester")]
        public void CTor_setting_Name_Parent_Owner_Culture_and_CultureInfo(string ownerName)
        {
            var name = "Property42";
            var parent = new Identifiable();
            var owner = GrainBaseTest.MakePrincipal(ownerName);
            var culture = CultureInfo.GetCultureInfo("en-US");
            var reference = new GrainLocalized(name, parent, owner, culture);
            var propDef = new GrainPropDef(name, parent, owner, culture);

            propDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(propDef);
        }

        [TestMethod]
        public void CTor_copying_IGrain()
        {
            var source = new GrainPlain()
            {
                Name = "Property42",
                Path = "marbas/Schema/Property42",
                Owner = "tester",
                ParentId = Guid.NewGuid()
            };
            var propDef = new GrainPropDef(source);

            propDef.Should().BeEquivalentTo<IGrain>(source,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(propDef);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CTor_copying_IPropDef(bool withValueContraint)
        {
            var source = new PropDefMock()
            {
                Name = "Property42",
                Path = "marbas/Schema/Property42",
                Owner = "tester",
                ParentId = Guid.NewGuid()
            };
            if (withValueContraint)
            {
                source.ValueConstraintId = Guid.NewGuid();
            }
            var propDef = new GrainPropDef(source);

            propDef.Should().BeEquivalentTo<IGrain>(source,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            propDef.Should().BeEquivalentTo<IPropDef>(source);
        }

        [TestMethod]
        public void CTor_copying_IGrainPropDef()
        {
            var source = new GrainPropDef("Property42", new Identifiable(), GrainBaseTest.MakePrincipal("tester"), CultureInfo.GetCultureInfo("en-US"))
            {
                CardinalityMin = 0,
                CardinalityMax = 42,
                Versionable = false,
                Localizable = false,
                ValueConstraint = new Identifiable(),
                ConstraintParams = "use=Nothing",
                ValueType = TraitValueType.Number
            };
            var propDef = new GrainPropDef(source);
            propDef.Should().BeEquivalentTo(source);
            propDef.GetDirtyFields<IGrainPropDef>().Should()
                .NotBeEmpty().And
                .BeEquivalentTo(source.GetDirtyFields<IGrainPropDef>());
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_ValueType_changing_ValueType(bool acceptAllChanges)
        {
            var propDef = new GrainPropDef();
            propDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            propDef.ValueType.Should().Be(TraitValueType.Text);

            propDef.ValueType = TraitValueType.Text;
            propDef.ValueType.Should().Be(TraitValueType.Text);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainPropDef>(propDef, acceptAllChanges, nameof(IValueTypeConstraint.ValueType));

            propDef.ValueType = TraitValueType.Boolean;
            propDef.ValueType.Should().Be(TraitValueType.Boolean);
            propDef.GetDirtyFields<IGrainPropDef>().Should().Satisfy(property => nameof(IValueTypeConstraint.ValueType) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_ValueConstraint_changing_ValueConstraint_and_ValueConstraintId(bool acceptAllChanges)
        {
            var propDef = new GrainPropDef();
            propDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            propDef.ValueConstraint.Should().BeNull();
            propDef.ValueConstraintId.Should().BeNull();

            propDef.ValueConstraint = null;
            propDef.ValueConstraintId.Should().BeNull();
            UpdateableTrackerTest.AssertFieldUpdates<IGrainPropDef>(propDef, acceptAllChanges, nameof(IGrainPropDef.ValueConstraint));

            propDef.ValueConstraint = new Identifiable();
            propDef.ValueConstraint.Should().NotBeNull();
            propDef.ValueConstraintId.Should().Be(propDef.ValueConstraint.Id);
            propDef.GetDirtyFields<IGrainPropDef>().Should().Satisfy(property => nameof(IGrainPropDef.ValueConstraint) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_ValueConstraintId_changing_ValueConstraint_and_ValueConstraintId(bool acceptAllChanges)
        {
            var propDef = new GrainPropDef();
            propDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            propDef.ValueConstraint.Should().BeNull();
            propDef.ValueConstraintId.Should().BeNull();

            ((IPropDef)propDef).ValueConstraintId = null;
            propDef.ValueConstraint.Should().BeNull();
            UpdateableTrackerTest.AssertFieldUpdates<IGrainPropDef>(propDef, acceptAllChanges, nameof(IGrainPropDef.ValueConstraint));

            ((IPropDef)propDef).ValueConstraintId = Guid.NewGuid();
            propDef.ValueConstraint.Should().NotBeNull();
            propDef.ValueConstraintId.Should().Be(propDef.ValueConstraint.Id);
            propDef.GetDirtyFields<IGrainPropDef>().Should().Satisfy(property => nameof(IGrainPropDef.ValueConstraint) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_ConstraintParams_changing_ConstraintParams(bool acceptAllChanges)
        {
            var propDef = new GrainPropDef();
            propDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            propDef.ConstraintParams.Should().BeNull();

            propDef.ConstraintParams = null;
            UpdateableTrackerTest.AssertFieldUpdates<IGrainPropDef>(propDef, acceptAllChanges, nameof(IPropDef.ConstraintParams));

            propDef.ConstraintParams = "use=Nothing";
            propDef.ConstraintParams.Should().NotBeNull();
            propDef.GetDirtyFields<IGrainPropDef>().Should().Satisfy(property => nameof(IPropDef.ConstraintParams) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_CardinalityMin_changing_CardinalityMin(bool acceptAllChanges)
        {
            var propDef = new GrainPropDef();
            propDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            propDef.CardinalityMin.Should().Be(1);

            propDef.CardinalityMin = 1;
            UpdateableTrackerTest.AssertFieldUpdates<IGrainPropDef>(propDef, acceptAllChanges, nameof(IPropDef.CardinalityMin));

            propDef.CardinalityMin = 42;
            propDef.CardinalityMin.Should().Be(42);
            propDef.GetDirtyFields<IGrainPropDef>().Should().Satisfy(property => nameof(IPropDef.CardinalityMin) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_CardinalityMax_changing_CardinalityMax(bool acceptAllChanges)
        {
            var propDef = new GrainPropDef();
            propDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            propDef.CardinalityMax.Should().Be(1);

            propDef.CardinalityMax = 1;
            UpdateableTrackerTest.AssertFieldUpdates<IGrainPropDef>(propDef, acceptAllChanges, nameof(IPropDef.CardinalityMax));

            propDef.CardinalityMax = 42;
            propDef.CardinalityMax.Should().Be(42);
            propDef.GetDirtyFields<IGrainPropDef>().Should().Satisfy(property => nameof(IPropDef.CardinalityMax) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Versionable_changing_Versionable(bool acceptAllChanges)
        {
            var propDef = new GrainPropDef();
            propDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            propDef.Versionable.Should().BeTrue();

            propDef.Versionable = true;
            UpdateableTrackerTest.AssertFieldUpdates<IGrainPropDef>(propDef, acceptAllChanges, nameof(IPropDef.Versionable));

            propDef.Versionable = false;
            propDef.Versionable.Should().BeFalse();
            propDef.GetDirtyFields<IGrainPropDef>().Should().Satisfy(property => nameof(IPropDef.Versionable) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Localizable_changing_Localizable(bool acceptAllChanges)
        {
            var propDef = new GrainPropDef();
            propDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            propDef.Localizable.Should().BeTrue();

            propDef.Localizable = true;
            UpdateableTrackerTest.AssertFieldUpdates<IGrainPropDef>(propDef, acceptAllChanges, nameof(IPropDef.Localizable));

            propDef.Localizable = false;
            propDef.Localizable.Should().BeFalse();
            propDef.GetDirtyFields<IGrainPropDef>().Should().Satisfy(property => nameof(IPropDef.Localizable) == property);
        }

        protected static void AssertDefaults(GrainPropDef propDef)
        {
            propDef.TypeDefId.Should().Be(SchemaDefaults.PropDefTypeDefID);
            propDef.TypeName.Should().Be(SchemaDefaults.PropDefTypeName);
            propDef.CardinalityMin.Should().Be(1);
            propDef.CardinalityMax.Should().Be(1);
            propDef.Versionable.Should().BeTrue();
            propDef.Localizable.Should().BeTrue();
            propDef.ValueConstraint.Should().BeNull();
            propDef.ValueConstraintId.Should().BeNull();
            propDef.ConstraintParams.Should().BeNull();
            propDef.ValueType.Should().Be(TraitValueType.Text);
        }
    }
}
