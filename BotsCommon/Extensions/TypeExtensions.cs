using System;

namespace BotsCommon
{
    public static class TypeExtensions
    {
        public static object? GetDefaultValue(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        public static Type GetElementTypeFromEnumerable(this Type type)
        {
            var enumerableType = type;

            if (type.Name != "IEnumerable`1")
                enumerableType = type.GetInterface("IEnumerable`1");

            if (enumerableType == null)
                return typeof(object);
            else
                return enumerableType.GenericTypeArguments[0];
        }
    }
}
