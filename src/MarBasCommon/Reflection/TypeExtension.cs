using System.Collections;
using System.Reflection;

namespace CraftedSolutions.MarBasCommon.Reflection
{
    public static class TypeExtension
    {
        public static Type GetEnumerableType(this Type? type, uint index = 0)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type) && index < type.GenericTypeArguments.Length)
            {
                return type.GenericTypeArguments[index];
            }
            return typeof(object);
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(this Type? type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
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
