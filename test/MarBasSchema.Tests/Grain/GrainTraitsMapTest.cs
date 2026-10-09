using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.Grain.Traits;

namespace CraftedSolutions.MarBasSchema.Tests.Grain
{
    [TestClass]
    public class GrainTraitsMapTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class PropDefMock(IIdentifiable other, string propName, TraitValueType valueType = TraitValueType.Text)
            : SimpleValueTypeContraint(other, valueType), INamed
        {
            public string Name { get; set; } = propName;
        }

        [TestMethod]
        public void Emplace_throwing_ArgumentException_given_property_name_is_missing()
        {
            var trait = new TraitText(new Identifiable(), new Identifiable());
            var map = new GrainTraitsMap();
           
            map.Invoking(map => map.Emplace(trait)).Should().Throw<ArgumentException>();
        }
        [TestMethod]
        public void Emplace_throwing_ArgumentException_given_property_name_is_empty()
        {
            var map = new GrainTraitsMap();
            var trait = new TraitText(new Identifiable(), new Identifiable());

            map.Invoking(map => map.Emplace(trait, string.Empty)).Should().Throw<ArgumentException>();
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Emplace_ignoring_ITraitBase_given_Value_is_null(bool useSeparateKey)
        {
            var key = "TestProperty";
            var trait = new TraitText(new Identifiable(), useSeparateKey ? new Identifiable() : new PropDefMock((Identifiable)Guid.NewGuid(), key));
            var map = new GrainTraitsMap();

            map.Should().BeEmpty();

            map.Emplace(trait, useSeparateKey ? key : null);
            map.Should().BeEmpty();
            map.GetValues(key).Should().BeNull();
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Emplace_creating_Values_given_single_ITraitBase(bool useSeparateKey)
        {
            var key = "TestProperty";
            var trait = new TraitText(new Identifiable(), useSeparateKey ? new Identifiable() : new PropDefMock((Identifiable)Guid.NewGuid(), key), "test value");
            var map = new GrainTraitsMap();

            map.Should().BeEmpty();
            map.ContainsKey(key).Should().BeFalse();

            map.Emplace(trait, useSeparateKey ? key : null);
            map.Should()
                .HaveCount(1).And
                .Satisfy(entry => entry.Key == key && null != entry.Value && 1 == entry.Value.Count && trait == entry.Value[0]);
            map.GetValues(key).Should().Satisfy(element => element == trait.Value);
            map[key].Should().BeEquivalentTo([trait]);
        }

        [TestMethod]
        public void Emplace_replacing_element_in_Values_given_existing_ITraitBse()
        {
            var key = "TestProperty";
            var trait = new TraitText(new Identifiable(), new PropDefMock((Identifiable)Guid.NewGuid(), key), "test value");
            var copy = new TraitText(trait);
            ((ITraitValue<string>)copy).Value = "copy value";
            var map = new GrainTraitsMap();

            map.Emplace(trait);
            map.Should().HaveCount(1);
            map.GetValues(key).Should().Satisfy(element => element == trait.Value);

            map.Emplace(copy);
            map.Should().HaveCount(1);
            map.GetValues(key).Should().Satisfy(element => element == copy.Value);
        }

        [TestMethod]
        public void Emplace_throwing_ArgumentException_given_incompatible_ITraitBsse()
        {
            var key = "TestProperty";
            var textTrait = TraitValueFactory.Create(TraitValueType.Text, new Identifiable(), new PropDefMock(new Identifiable(), key), "test value");
            var numberTrait = TraitValueFactory.Create(TraitValueType.Number, new Identifiable(), new PropDefMock(new Identifiable(), key, TraitValueType.Number), 42);
            var map = new GrainTraitsMap();

            map.Emplace(textTrait);
            map.Should().HaveCount(1);

            map.Invoking((map) => {
                map.Emplace(numberTrait);
                }).Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Emplace_populating_ordered_Values_given_multiple_ITraitBase()
        {
            var prop = new PropDefMock(new Identifiable(), "TextProperty", TraitValueType.Text);
            var map = new GrainTraitsMap();

            for (int i = 4; i >= 0; i--)
            {
                var trait = new TraitText(new Identifiable(), prop, $"value {i}")
                {
                    Ord = i
                };
                map.Emplace(trait);
            }
            map.Should().HaveCount(1);
            map.ContainsKey(prop.Name).Should().BeTrue();
            var traits = map[prop.Name];
            traits.Should().NotBeNull().And.HaveCount(5);
            for (var i = 0; i < traits.Count; i++)
            {
                traits[i].Should().NotBeNull();
                traits[i].Ord.Should().Be(i);
            }
        }

        [TestMethod]
        public void Emplace_populating_Values_given_multiple_ITraitBase_with_different_ValueType()
        {
            var props = new PropDefMock[]
            {
                new(new Identifiable(), "TextProperty", TraitValueType.Text),
                new(new Identifiable(), "NumberProperty", TraitValueType.Number)
            };
            var grain = new Identifiable();
            var map = new GrainTraitsMap();
            for (var i = 0; i < 10; i++)
            {
                var prop = props[i % 2];
                var trait = TraitValueFactory.Create(prop.ValueType, grain, prop, TraitValueType.Number == prop.ValueType ? i : $"value {i}");

                map.Emplace(trait);
            }

            map.Should().HaveCount(2);
            map.Keys.Should().Satisfy(
                key => key == props[0].Name,
                key => key == props[1].Name
                );

            var textValues = map.GetValues<string>(props[0].Name);
            textValues.Should().HaveCount(5);
            for (var i = 0; i <  textValues.Length; i++)
            {
                textValues.Should().HaveElementAt(i, $"value {i * 2}");
            }

            var numberValues = map.GetValues<decimal>(props[1].Name);
            numberValues.Should().HaveCount(5);
            for (var i = 0; i < numberValues.Length; i++)
            {
                numberValues.Should().HaveElementAt(i, i * 2 + 1);
            }
        }

        [TestMethod]
        public void GetValues_returning_null_given_nonexisting_key()
        {
            var key = "imaginary";
            var map = new GrainTraitsMap();
            map.GetValues(key).Should().BeNull();
            map.GetValues<Guid?>(key).Should().BeNull();
        }

        [TestMethod]
        public void GetValues_returning_null_given_key_pointing_to_null()
        {
            var key = "imaginary";
            var map = new GrainTraitsMap();
            map.Should().BeEmpty();
            map[key] = null;
            map.Should().NotBeEmpty();
            map.GetValues(key).Should().BeNull();
            map.GetValues<Guid?>(key).Should().BeNull();
        }

        [TestMethod]
        public void GetValues_returning_empty_Array_given_Type_mismatch()
        {
            var key = "TestProperty";
            var trait = new TraitText(new Identifiable(), new PropDefMock((Identifiable)Guid.NewGuid(), key), "test value");
            var map = new GrainTraitsMap();

            map.Emplace(trait);
            map.GetValues<bool>(key).Should().BeEmpty();
        }
    }
}
