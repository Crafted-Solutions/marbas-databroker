using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.Grain.Traits;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Tests.Grain.Traits
{
    [TestClass]
    public class TraitValueTest
    {
        [TestMethod]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        public void CTor_throwing_InvalidCastException_given_PropDef_with_wrong_ValueType(TraitValueType initType)
        {
            var initCall = () => TraitValueFactory.Create(initType, new Identifiable(), new Identifiable());
            initCall.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void CTor_throwing_ArgumentException_given_incompatible_Type_of_value()
        {
            var initCall = () => new TraitValue<string>(new Identifiable(), new Identifiable(), 42d);
            initCall.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void CTor_setting_Grain_GrainId_PropDef_PropDefId_and_Value()
        {
            var grain = new Identifiable();
            var propDef = new SimpleValueTypeContraint(new Identifiable(), TraitValueType.Memo);
            var value = "test value";
            var trait = new TraitValue<string>(grain, propDef, value);

            trait.Grain.Should().Be(grain);
            trait.GrainId.Should().Be(grain.Id);
            trait.PropDef.Should().Be(propDef);
            trait.PropDefId.Should().Be(propDef.Id);
            trait.IsNull.Should().BeFalse();
            trait.ValueType.Should().Be(TraitValueType.Memo);
            trait.Value.Should().Be(value);
            trait.GetDirtyFields<ITraitBase>().Should().BeEmpty();
        }

        [TestMethod]
        public void CTor_setting_Grain_GrainId_PropDef_PropDefId_Value_Culture_and_CultureInfo()
        {
            var grain = new Identifiable();
            var propDef = new SimpleValueTypeContraint(new Identifiable(), TraitValueType.Number);
            var value = 42;
            var culture = CultureInfo.GetCultureInfo("en-UK");
            var trait = new TraitValue<decimal>(grain, propDef, value, culture);

            trait.Grain.Should().Be(grain);
            trait.GrainId.Should().Be(grain.Id);
            trait.PropDef.Should().Be(propDef);
            trait.PropDefId.Should().Be(propDef.Id);
            trait.IsNull.Should().BeFalse();
            trait.ValueType.Should().Be(TraitValueType.Number);
            trait.Value.Should().Be(Convert.ToDecimal(value));
            trait.CultureInfo.Should().Be(culture);
            trait.Culture.Should().Be(culture.Name);
            trait.GetDirtyFields<ITraitBase>().Should().BeEmpty();
        }

        [TestMethod]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void CTor_throwing_InvalidCastException_given_incompatible_ITrait(TraitValueType initType)
        {
            var propDef = new SimpleValueTypeContraint(new Identifiable(), initType);
            var source = new TraitBaseMock(new Identifiable(), propDef);

            var initCall = () => new TraitValue<string>(source);
            initCall.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void CTor_copying_ITraitBase()
        {
            var source = new TraitBaseMock(new Identifiable(), new Identifiable(), CultureInfo.GetCultureInfo("de-DE"))
            {
                OwnValue = "test value"
            };
            var trait = new TraitValue<string>(source);
            trait.Should().BeEquivalentTo<TraitBase>(source);
        }

        [TestMethod]
        public void CTor_copying_ITrait()
        {
            var source = new TraitMock()
            {
                Value = true,
                ValueType = TraitValueType.Boolean
            };
            var trait = new TraitValue<bool>(source);
            trait.Should().BeEquivalentTo<ITrait>(source);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Grain_changing_Grain_and_GrainId(bool acceptAllChanges)
        {
            var grain = new Identifiable();
            var trait = new TraitValue<DateTime>(grain, new SimpleValueTypeContraint(new Identifiable(), TraitValueType.DateTime), null);
            trait.FieldTracker.AcceptAllChanges = acceptAllChanges;

            trait.Grain.Should().BeSameAs(grain);

            trait.Grain = grain;
            trait.GrainId.Should().Be(grain.Id);
            UpdateableTrackerTest.AssertFieldUpdates<ITraitBase>(trait, acceptAllChanges, nameof(IGrainBinding.Grain));

            trait.Grain = new Identifiable();
            trait.Grain.Should().NotBe(grain);
            trait.GrainId.Should().NotBe(grain);
            trait.GetDirtyFields<ITraitBase>().Should().Satisfy(property => nameof(IGrainBinding.Grain) == property);
        }

        [TestMethod]
        public void Set_PropDef_throwing_InvalidCastException_given_wrong_ValueType()
        {
            var trait = new TraitValue<bool>(new Identifiable(), new SimpleValueTypeContraint(new Identifiable(), TraitValueType.Boolean), false);
            var setter = () => trait.PropDef = new Identifiable();
            setter.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_PropDef_changing_PropDef_and_PropDefId(bool acceptAllChanges)
        {
            var propDef = new SimpleValueTypeContraint(new Identifiable(), TraitValueType.Grain);
            var trait = new TraitValue<Guid?>(new Identifiable(), propDef, Guid.NewGuid());
            trait.FieldTracker.AcceptAllChanges = acceptAllChanges;

            trait.PropDef.Should().BeSameAs(propDef);

            trait.PropDef = propDef;
            trait.PropDefId.Should().Be(propDef.Id);
            UpdateableTrackerTest.AssertFieldUpdates<ITraitBase>(trait, acceptAllChanges, nameof(ITraitRef.PropDef));

            trait.PropDef = new SimpleValueTypeContraint(new Identifiable(), TraitValueType.Grain);
            trait.PropDef.Should().NotBe(propDef);
            trait.PropDefId.Should().NotBe(propDef.Id);
            trait.GetDirtyFields<ITraitBase>().Should().Satisfy(property => nameof(ITraitRef.PropDef) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_CultureInfo_changing_CultureInfo_and_Culture(bool acceptAllChanges)
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var trait = new TraitText(new Identifiable(), new Identifiable(), "test value", culture);
            trait.FieldTracker.AcceptAllChanges = acceptAllChanges;

            trait.CultureInfo.Should().BeSameAs(culture);

            trait.CultureInfo = culture;
            trait.Culture.Should().Be(culture.Name);
            UpdateableTrackerTest.AssertFieldUpdates<ITraitBase>(trait, acceptAllChanges, nameof(ILocalizable.CultureInfo));

            trait.CultureInfo = CultureInfo.InvariantCulture;
            trait.CultureInfo.Should().NotBe(culture);
            trait.Culture.Should().NotBe(culture.Name);
            trait.GetDirtyFields<ITraitBase>().Should().Satisfy(property => nameof(ILocalizable.CultureInfo) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Ord_changing_Ord(bool acceptAllChanges)
        {
            var trait = new TraitFile(new Identifiable(), new SimpleValueTypeContraint(new Identifiable(), TraitValueType.File));
            trait.FieldTracker.AcceptAllChanges = acceptAllChanges;

            trait.Ord.Should().Be(0);

            trait.Ord = 0;
            trait.Ord.Should().Be(0);
            UpdateableTrackerTest.AssertFieldUpdates<ITraitBase>(trait, acceptAllChanges, nameof(ITrait.Ord));

            trait.Ord = 42;
            trait.Ord.Should().Be(42);
            trait.GetDirtyFields<ITraitBase>().Should().Satisfy(property => nameof(ITrait.Ord) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Revision_changing_Revision(bool acceptAllChanges)
        {
            var trait = new TraitValue<string>(new Identifiable(), new SimpleValueTypeContraint(new Identifiable(), TraitValueType.Memo), "test value");
            trait.FieldTracker.AcceptAllChanges = acceptAllChanges;

            trait.Revision.Should().Be(1);

            trait.Revision = 1;
            trait.Revision.Should().Be(1);
            UpdateableTrackerTest.AssertFieldUpdates<ITraitBase>(trait, acceptAllChanges, nameof(ITrait.Revision));

            trait.Revision = 42;
            trait.Revision.Should().Be(42);
            trait.GetDirtyFields<ITraitBase>().Should().Satisfy(property => nameof(ITrait.Revision) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Value_changing_Value(bool acceptAllChanges)
        {
            var value = Guid.NewGuid();
            var trait = new TraitValue<Guid?>(new Identifiable(), new SimpleValueTypeContraint(new Identifiable(), TraitValueType.Grain), value);
            trait.FieldTracker.AcceptAllChanges = acceptAllChanges;

            trait.Value.Should().Be(value);

            trait.AsWritable().Value = value;
            trait.Value.Should().Be(value);
            UpdateableTrackerTest.AssertFieldUpdates<ITraitBase>(trait, acceptAllChanges, nameof(ITrait.Value));

            trait.AsWritable().Value = Guid.NewGuid();
            trait.Value.Should().NotBe(value);
            trait.GetDirtyFields<ITraitBase>().Should().Satisfy(property => nameof(ITrait.Value) == property);
        }

    }
}
