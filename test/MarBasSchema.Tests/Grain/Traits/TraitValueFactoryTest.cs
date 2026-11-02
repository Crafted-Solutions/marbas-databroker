using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.Grain.Traits;
using System.Globalization;
using System.Text.Json;

namespace CraftedSolutions.MarBasSchema.Tests.Grain.Traits
{
    [TestClass]
    public class TraitValueFactoryTest
    {
        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void Create_returning_ITraitBase_given_ITrait(TraitValueType valueType)
        {
            var template = new TraitMock()
            {
                PropDef = new SimpleValueTypeContraint(new Identifiable(), valueType),
                ValueType = valueType
            };
            var trait = TraitValueFactory.Create(template);
            trait.Should().BeEquivalentTo<ITrait>(template, options => options.ExcludingMembersNamed(nameof(ITrait.Value), nameof(ITrait.IsNull)));
        }

        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void Create_returning_ITraitBase_given_ITrait_and_value(TraitValueType valueType)
        {
            var value = MakeTraitValue(valueType);
            var template = new TraitMock()
            {
                PropDef = new SimpleValueTypeContraint(new Identifiable(), valueType),
                Value = value,
                ValueType = valueType
            };
            var trait = TraitValueFactory.Create(template, value);
            trait.Should().BeEquivalentTo<ITrait>(template);
        }

        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void Create_returning_ITraitBase_given_ITraitRef(TraitValueType valueType)
        {
            var traitRef = new TraitRef(new Identifiable(),
                TraitValueType.Text == valueType ? new Identifiable() : new SimpleValueTypeContraint(new Identifiable(), valueType));
            var trait = TraitValueFactory.Create(traitRef);
            trait.Should().BeEquivalentTo<ITraitRef>(trait);
            trait.ValueType.Should().Be(valueType);
        }

        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void Create_returning_ITraitBase_given_ITraitRef_and_value(TraitValueType valueType)
        {
            var value = MakeTraitValue(valueType);
            var traitRef = new TraitRef(new Identifiable(),
                TraitValueType.Text == valueType ? new Identifiable() : new SimpleValueTypeContraint(new Identifiable(), valueType));
            var trait = TraitValueFactory.Create(traitRef, value);
            trait.Should().BeEquivalentTo<ITraitRef>(trait);
            trait.ValueType.Should().Be(valueType);
            trait.Value.Should().Be(value);
        }

        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void ConvertValue_falling_back_to_System_Convert_given_null(TraitValueType valueType)
        {
            var converted = TraitValueFactory.ConvertValue(valueType);

            converted.Should().Be(valueType switch
            {
                TraitValueType.Boolean => false,
                TraitValueType.Number => 0m,
                TraitValueType.DateTime => new DateTime(0),
                TraitValueType.Grain or TraitValueType.File => null,
                _ => string.Empty
            });
        }

        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void ConvertValue_returning_value_as_is(TraitValueType valueType)
        {
            var value = MakeTraitValue(valueType, true);
            var converted = TraitValueFactory.ConvertValue(valueType, value);

            converted.Should().Be(value);
        }

        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void ConvertValue_returning_value_from_string(TraitValueType valueType)
        {
            var value = MakeTraitValue(valueType);
            var converted = TraitValueFactory.ConvertValue(valueType, value?.ToString());
            
            if (converted is IIdentifiable identifiable)
            {
                identifiable.Id.Should().Be((Guid)value!);
            }
            else
            {
                converted.Should().Be(value);
            }
        }

        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void ConvertValue_returning_value_from_JsonElement(TraitValueType valueType)
        {
            var value = MakeTraitValue(valueType);
            string strVal;
            if (value is Guid || value is string || value is DateTime)
            {
                strVal = $"\"{value}\"";
            }
            else if (value is bool)
            {
                strVal = $"{value?.ToString()?.ToLowerInvariant()}";
            }
            else
            {
                strVal = $"{value}";
            }
            var converted = TraitValueFactory.ConvertValue(valueType, JsonElement.Parse(strVal));

            if (converted is IIdentifiable identifiable)
            {
                identifiable.Id.Should().Be((Guid)value!);
            }
            else
            {
                converted.Should().Be(value);
            }
        }

        [TestMethod]
        public void GetValueTypeFromString_returning_TraitValueType_Text_given_null()
        {
            var valueType = TraitValueFactory.GetValueTypeFromString(null);
            valueType.Should().Be(TraitValueType.Text);
        }

        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void GetValueTypeFromString_returning_TraitValueType(TraitValueType valueType)
        {
            var converted = TraitValueFactory.GetValueTypeFromString(Enum.GetName(valueType));
            converted.Should().Be(valueType);

            converted = TraitValueFactory.GetValueTypeFromString(Enum.GetName(valueType)!.ToLowerInvariant());
            converted.Should().Be(valueType);

        }

        [TestMethod]
        [DataRow(TraitValueType.Text)]
        [DataRow(TraitValueType.Memo)]
        [DataRow(TraitValueType.Boolean)]
        [DataRow(TraitValueType.Number)]
        [DataRow(TraitValueType.DateTime)]
        [DataRow(TraitValueType.Grain)]
        [DataRow(TraitValueType.File)]
        public void GetValueTypeAsString_returning_TraitValueType_name(TraitValueType valueType)
        {
            var strValueType = TraitValueFactory.GetValueTypeAsString(valueType);
            strValueType.Should().Be(Enum.GetName(valueType)!.ToLowerInvariant());
        }

        private static object? MakeTraitValue(TraitValueType valueType, bool guidAsIdentifiable = false)
        {
            var type = TraitValueFactory.GetValueNativeType(valueType);
            object? result = null;
            if (TraitValueType.Grain == valueType || TraitValueType.File == valueType)
            {
                result = Guid.NewGuid();
                if (guidAsIdentifiable)
                {
                    result = (Identifiable)(Guid)result;
                }
            }
            else if (type.IsValueType)
            {
                result = Activator.CreateInstance(type);
            }
            else if (type == typeof(string))
            {
                result = "nothing";
            }
            return result;
        }
    }
}
