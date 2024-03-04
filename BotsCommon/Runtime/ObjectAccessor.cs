#nullable enable

using System.Reflection;

namespace BotsCommon.Runtime
{
    public sealed class ObjectAccessor
    {
        private static readonly MemberAccessor _generickParameterCountAccessor = new(
             typeof(MethodInfo).GetProperty("GenericParameterCount", BindingFlags.NonPublic | BindingFlags.Instance)!
        );

        public ObjectAccessor(object? obj, Type type)
        {
            Instance = obj;
            Type = type;
        }

        public object? Instance { get; }
        public Type Type { get; }

        public object? GetProperty(string name)
        {
            var accessor = GetPropertyAccessor(name);

            return accessor.GetValue(
                ((PropertyInfo)accessor.MemberInfo).GetMethod!.IsStatic ? null : Instance
            );
        }

        public object? GetProperty(PropertyInfo property)
        {
            return MemberAccessorsCache.Shared.Create(property).GetValue(
                property.GetMethod!.IsStatic ? null : Instance
            );
        }

        public void SetProperty(string name, object? value)
        {
            var accessor = GetPropertyAccessor(name);

            accessor.SetValue(
                ((PropertyInfo)accessor.MemberInfo).SetMethod!.IsStatic ? null : Instance,
                value
            );
        }

        public void SetProperty(PropertyInfo property, object? value)
        {
            MemberAccessorsCache.Shared.Create(property).SetValue(
                property.SetMethod!.IsStatic ? null : Instance,
                value
            );
        }

        private MemberAccessor GetPropertyAccessor(string name)
        {
            return Memoizer<ObjectAccessor>.Shared.GetOrAddMemoizedValue(
                () =>
                {
                    var property = Type.GetRuntimeProperty(name) ??
                        throw new Exception($"Property with name {name} not found");

                    return MemberAccessorsCache.Shared.Create(property);
                },
                Type,
                name
            );
        }

        public object? GetField(string name)
        {
            var accessor = GetFieldAccessor(name);

            return accessor.GetValue(
                ((FieldInfo)accessor.MemberInfo).IsStatic ? null : Instance
            );
        }

        public object? GetField(FieldInfo field)
        {
            return MemberAccessorsCache.Shared.Create(field).GetValue(
                field.IsStatic ? null : Instance
            );
        }

        public void SetField(string name, object? value)
        {
            var accessor = GetFieldAccessor(name);

            accessor.SetValue(
                ((FieldInfo)accessor.MemberInfo).IsStatic ? null : Instance,
                value
            );
        }

        public void SetField(FieldInfo field, object? value)
        {
            MemberAccessorsCache.Shared.Create(field).SetValue(
                field.IsStatic ? null : Instance,
                value
            );
        }

        private MemberAccessor GetFieldAccessor(string name)
        {
            return Memoizer<ObjectAccessor>.Shared.GetOrAddMemoizedValue(
                () =>
                {
                    var field = Type.GetRuntimeField(name) ??
                        throw new Exception($"Field with name {name} not found");

                    return MemberAccessorsCache.Shared.Create(field);
                },
                Type,
                name
            );
        }

        public object? CallMethod(string name, params object?[] args)
        {
            return CallMethod(name, Type.EmptyTypes, args);
        }

        public object? CallMethod(string name, Type[] genericTypes, params object?[] args)
        {
            return CallMethod(name, genericTypes, args.Select(x => x?.GetType()).ToArray(), args);
        }

        public object? CallMethod(string name, Type[] genericTypes, Type?[] argTypes, params object?[] args)
        {
            var accessor = GetMethodAccessor(name, genericTypes, argTypes);

            return accessor.Invoke(
                ((MethodInfo)accessor.MemberInfo).IsStatic ? null : Instance,
                args
            );
        }

        private MemberAccessor GetMethodAccessor(string name, Type[] genericTypes, Type?[] argTypes)
        {
            return Memoizer<ObjectAccessor>.Shared.GetOrAddMemoizedValue(
                () =>
                {
                    var method = Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                        .Where(x => x.Name == name)
                        .Where(x => (int)_generickParameterCountAccessor.GetValue(x)! == genericTypes.Length)
                        .Select(x => x.IsGenericMethodDefinition ? x.MakeGenericMethod(genericTypes) : x)
                        .Where(x => x.GetParameters().Length == argTypes.Length)
                        .Where(x => x
                            .GetParameters()
                            .Select((c, i) => (c, i))
                            .All(c => argTypes[c.i] == null || c.c.ParameterType.IsAssignableFrom(argTypes[c.i]))
                        )
                        .FirstOrDefault() ??
                        throw new Exception($"Method with name {name}, generic types {string.Join(", ", genericTypes.Select(x => x.Name))} and arg types {string.Join(", ", argTypes.Select(x => x?.Name ?? "null"))} not found");

                    return MemberAccessorsCache.Shared.Create(method);
                },
                Type,
                name,
                genericTypes,
                argTypes
            );
        }

        public static ObjectAccessor Create(Type type, params object?[] args)
        {
            return Create(type, args.Select(x => x!.GetType()).ToArray(), args);
        }

        public static ObjectAccessor Create(Type type, Type[] argTypes, params object?[] args)
        {
            var constructorAccessor = Memoizer<ObjectAccessor>.Shared.GetOrAddMemoizedValue(
                () =>
                {
                    var constructor = type
                        .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(x => x.GetParameters().Length == argTypes.Length)
                        .Where(x => x
                            .GetParameters()
                            .Select((c, i) => (c, i))
                            .All(c => c.c.ParameterType.IsAssignableFrom(argTypes[c.i]))
                        )
                        .FirstOrDefault() ??
                        throw new Exception($"Constructor on type {type} not found for arg types {string.Join(", ", argTypes.Select(x => x.Name))}");

                    return MemberAccessorsCache.Shared.Create(constructor);
                },
                type,
                argTypes
            );

            var obj = constructorAccessor.Invoke(null, args);

            return new ObjectAccessor(obj, type);
        }

        public static ObjectAccessor CreateStatic(Type type)
        {
            return new ObjectAccessor(null, type);
        }
    }
}
