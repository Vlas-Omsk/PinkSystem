using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PinkSystem.Runtime
{
    public static class TypeConverter
    {
        private static readonly ConcurrentDictionary<int, bool> _isArrayTypeCache = new();
        private static readonly ConcurrentDictionary<int, bool> _isAssignableToCache = new();
        private static readonly ConcurrentDictionary<int, bool> _isPrimitiveTypeCache = new();
        private static readonly HashSet<Type> _primitiveTypes = new()
        {
            typeof(string),
            typeof(decimal),
        };

        public static object? ChangeType(object? value, Type targetType)
        {
            if (!TryChangeType(value, targetType, out var result))
                throw new ArgumentException($"Cannot convert value of type {value?.GetType()} to type {targetType}");

            return result;
        }

        public static bool TryChangeType(object? value, Type targetType, out object? result)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (value == null)
            {
                result = value;
                return true;
            }

            var underlyingTargetType = Nullable.GetUnderlyingType(targetType);

            if (underlyingTargetType != null)
                targetType = underlyingTargetType;

            var valueType = value.GetType();

            if (IsEqualsOrAssignableToCached(valueType, targetType))
            {
                result = value;
                return true;
            }

            if (targetType == typeof(object))
            {
                result = value;
                return true;
            }

            try
            {
                result = Convert.ChangeType(value, targetType);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static bool IsPrimitiveType(Type type)
        {
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();

            var hash = HashCode.Combine(type);

            if (_isPrimitiveTypeCache.TryGetValue(hash, out var result))
                return result;

            result =
                type.IsPrimitive ||
                type.IsEnum ||
                _primitiveTypes.Contains(type);

            _isPrimitiveTypeCache.TryAdd(hash, result);
            return result;
        }

        public static void AddPrimitiveType(Type type)
        {
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();

            _isPrimitiveTypeCache.Clear();
            _primitiveTypes.Add(type);
        }

        public static void RemovePrimitiveType(Type type)
        {
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();

            _isPrimitiveTypeCache.Clear();
            _primitiveTypes.Remove(type);
        }

        public static bool IsArrayType(Type type)
        {
            var hash = HashCode.Combine(type);

            if (_isArrayTypeCache.TryGetValue(hash, out var result))
                return result;

            result =
                type.GetInterface(nameof(IEnumerable)) != null &&
                type != typeof(string);

            _isArrayTypeCache.TryAdd(hash, result);
            return result;
        }

        public static bool IsValueType(Type type)
        {
            return type.IsValueType && !type.IsPrimitive || type == typeof(string);
        }

        public static bool IsAnonymousType(Type type)
        {
            return
                Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false) &&
                (type.IsGenericType || IsEmptyAnonymousType(type)) &&
                type.Name.Contains("AnonymousType") &&
                (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$")) &&
                (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public static bool IsEmptyAnonymousType(Type type)
        {
            var name = type.Name;
            while (char.IsDigit(name[name.Length - 1]))
                name = name.Substring(0, name.Length - 1);
            return name == "<>f__AnonymousType";
        }

        private static bool IsEqualsOrAssignableToCached(Type type, Type targetType)
        {
            return
                type == targetType ||
                IsAssignableToCached(type, targetType);
        }

        private static bool IsAssignableToCached(Type sourceType, Type targetType)
        {
            var hash = HashCode.Combine(sourceType, targetType);

            if (_isAssignableToCache.TryGetValue(hash, out var result))
                return result;

            result = sourceType.IsAssignableTo(targetType);

            _isAssignableToCache.TryAdd(hash, result);
            return result;
        }


#if !NET5_0_OR_GREATER
        private static bool IsAssignableTo(this Type sourceType, Type targetType) => targetType?.IsAssignableFrom(sourceType) ?? false;
#endif
    }
}
