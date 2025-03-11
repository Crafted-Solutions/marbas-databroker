using System.Globalization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasCommon.Reflection;

namespace CraftedSolutions.MarBasSchema.Grain.Traits
{
    public static class TraitValueFactory
    {
        public static ITraitBase Create(ITraitBase template, object? value = null)
        {
            ITraitBase result;
            switch (template.ValueType)
            {
                case TraitValueType.Boolean:
                    result = new TraitValue<bool>(template);
                    ((ITraitValue<bool>)result).Value = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                    break;
                case TraitValueType.Number:
                    result = new TraitValue<decimal>(template);
                    ((ITraitValue<decimal>)result).Value = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                    break;
                case TraitValueType.DateTime:
                    result = new TraitValue<DateTime>(template);
                    ((ITraitValue<DateTime>)result).Value = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                    break;
                case TraitValueType.Grain:
                    result = new TraitValue<Guid?>(template);
                    ((ITraitValue<Guid?>)result).Value = GetIdentifiableFromValue(value)?.Id;
                    break;
                case TraitValueType.File:
                    result = new TraitFile(template);
                    ((ITraitValue<Guid?>)result).Value = GetIdentifiableFromValue(value)?.Id;
                    break;
                default:
                    result = new TraitText(template);
                    ((ITraitValue<string>)result).Value = Convert.ToString(value, CultureInfo.InvariantCulture);
                    break;
            }
            return result;
        }

        public static ITraitBase Create(ITraitRef traitRef, object? value = null)
        {
            var valueType = TraitValueType.Text;
            if (traitRef.PropDef is IValueTypeConstraint constraint)
            {
                valueType = constraint.ValueType;
            }
            return Create(valueType, traitRef.Grain, traitRef.PropDef, value, traitRef.CultureInfo);
        }

        public static ITraitBase Create(TraitValueType valueType, IIdentifiable grain, IIdentifiable propdef, object? value = null, CultureInfo? culture = null)
        {
            return valueType switch
            {
                TraitValueType.Boolean => new TraitValue<bool>(grain, propdef, Convert.ToBoolean(value, CultureInfo.InvariantCulture), culture),
                TraitValueType.Number => new TraitValue<decimal>(grain, propdef, Convert.ToDecimal(value, CultureInfo.InvariantCulture), culture),
                TraitValueType.DateTime => new TraitValue<DateTime>(grain, propdef, Convert.ToDateTime(value, CultureInfo.InvariantCulture), culture),
                TraitValueType.Grain => new TraitValue<Guid?>(grain, propdef, GetIdentifiableFromValue(value)?.Id, culture),
                TraitValueType.File => new TraitFile(grain, propdef, GetIdentifiableFromValue(value), culture),
                _ => new TraitText(grain, propdef, Convert.ToString(value, CultureInfo.InvariantCulture), culture),
            };
        }

        public static object? ConvertValue(TraitValueType valueType, object? value = null)
        {
            var val = value?.CastUnparsedJson();
            return valueType switch
            {
                TraitValueType.Boolean => Convert.ToBoolean(val, CultureInfo.InvariantCulture),
                TraitValueType.Number => Convert.ToDecimal(val, CultureInfo.InvariantCulture),
                TraitValueType.DateTime => Convert.ToDateTime(val, CultureInfo.InvariantCulture),
                TraitValueType.Grain or TraitValueType.File => GetIdentifiableFromValue(val),
                _ => Convert.ToString(val, CultureInfo.InvariantCulture)
            };
        }

        public static IIdentifiable? GetIdentifiableFromValue(object? value)
        {
            IIdentifiable? result = null;
            if (value is Guid guid)
            {
                result = (Identifiable)guid;
            }
            else if (value is IIdentifiable id)
            {
                result = id;
            }
            else if (null != value)
            {
                result = (Identifiable)Guid.Parse(value.ToString()!);
            }
            return result;
        }

        public static TraitValueType GetValueTypeFromString(string? valueType)
        {
            return Enum.Parse<TraitValueType>(valueType ?? Enum.GetName(TraitValueType.Text)!, true);
        }

        public static string GetValueTypeAsString(TraitValueType value)
        {
            return value.ToString().ToLowerInvariant();
        }

        public static Type GetValueNativeType(TraitValueType valueType)
        {
            return valueType switch
            {
                TraitValueType.Boolean => typeof(bool),
                TraitValueType.Number => typeof(decimal),
                TraitValueType.DateTime => typeof(DateTime),
                TraitValueType.Grain or TraitValueType.File => typeof(Guid),
                _ => typeof(string)
            };
        }
    }
}
