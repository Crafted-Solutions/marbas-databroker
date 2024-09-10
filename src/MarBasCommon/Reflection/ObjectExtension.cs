using System.Reflection;
using System.Text.Json;

namespace MarBasCommon.Reflection
{
    public static class ObjectExtension
    {
        public static T CastTo<T>(this object o) => (T)o;

        public static dynamic? CastToReflected(this object o, Type type)
        {
            var methodInfo = typeof(ObjectExtension).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
            var genericArguments = new[] { type };
            var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
            return genericMethodInfo?.Invoke(null, new[] { o });
        }

        public static object? CastUnparsedJson(this object o)
        {
            if (typeof(JsonElement).IsAssignableFrom(o.GetType()))
            {
                var elm = (dynamic)o;
                return elm.ValueKind switch
                {
                    JsonValueKind.Number => elm.GetDecimal(),
                    JsonValueKind.False or JsonValueKind.True => elm.GetBoolean(),
                    JsonValueKind.Null or JsonValueKind.Undefined => null,
                    _ => elm.GetString()
                };
            }
            return o;
        }
    }
}
