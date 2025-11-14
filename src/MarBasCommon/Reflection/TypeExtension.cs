using System.Collections;
using System.Reflection;

namespace CraftedSolutions.MarBasCommon.Reflection
{
    public static class TypeExtension
    {
        public static Type GetEnumerableType(this Type? type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type) && 0 < type.GenericTypeArguments.Length)
            {
                return type.GenericTypeArguments[0];
            }
            return typeof(object);
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(this Type? type, BindingFlags bindingFlags = BindingFlags.Public)
        {
            if (null == type)
            {
                return [];
            }
            if (!type.IsInterface || !bindingFlags.HasFlag(BindingFlags.FlattenHierarchy))
            {
                return type.GetProperties(bindingFlags);
            }

            return (new Type[] { type })
                   .Concat(type.GetInterfaces())
                   .SelectMany(i => i.GetProperties(bindingFlags));
        }
    }
}
