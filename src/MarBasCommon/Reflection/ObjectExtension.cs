using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace CraftedSolutions.MarBasCommon.Reflection
{
    public static class ObjectExtension
    {
        public static T CastTo<T>(this object o) => (T)o;

        public static dynamic? CastToReflected(this object o, Type type)
        {
            var methodInfo = typeof(ObjectExtension).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
            var genericArguments = new[] { type };
            var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
            return genericMethodInfo?.Invoke(null, [o]);
        }

        public static object? CastUnparsedJson([AllowNull] this object? o)
        {
            if (null != o && typeof(JsonElement).IsAssignableFrom(o.GetType()))
            {
                var elm = (dynamic)o;
                return elm.ValueKind switch
                {
                    JsonValueKind.Number => elm.GetDecimal(),
                    JsonValueKind.String => elm.GetString(),
                    JsonValueKind.False or JsonValueKind.True => elm.GetBoolean(),
                    JsonValueKind.Null or JsonValueKind.Undefined => null,
                    _ => throw new NotSupportedException("Only primitive JSON types are supported")
                };
            }
            return o;
        }

        public static T CastOrThrow<T>(this object o)
        {
            if (o is T desired)
            {
                return desired;
            }
            throw new ArgumentException($"{o.GetType()?.Name} misses required capabilities ({typeof(T).Name})");
        }
    }
}
