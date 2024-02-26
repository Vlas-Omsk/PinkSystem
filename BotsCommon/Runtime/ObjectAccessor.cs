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
            Object = obj;
            Type = type;
        }

        public object? Object { get; }
        public Type Type { get; }

        public object? GetProperty(string name)
        {
            return GetPropertyAccessor(name).GetValue(Object);
        }

        public object? GetProperty(PropertyInfo property)
        {
            return MemberAccessorsCache.Shared.Create(property).GetValue(Object);
        }

        public void SetProperty(string name, object? value)
        {
            GetPropertyAccessor(name).SetValue(Object, value);
        }

        public void SetProperty(PropertyInfo property, object? value)
        {
            MemberAccessorsCache.Shared.Create(property).SetValue(Object, value);
        }

        private MemberAccessor GetPropertyAccessor(string name)
        {
            var property = Type.GetRuntimeProperty(name) ??
                throw new Exception($"Property with name {name} not found");

            return MemberAccessorsCache.Shared.Create(property);
        }

        public object? GetField(string name)
        {
            return GetFieldAccessor(name).GetValue(Object);
        }

        public object? GetField(FieldInfo field)
        {
            return MemberAccessorsCache.Shared.Create(field).GetValue(Object);
        }

        public void SetField(string name, object? value)
        {
            GetFieldAccessor(name).SetValue(Object, value);
        }

        public void SetField(FieldInfo field, object? value)
        {
            MemberAccessorsCache.Shared.Create(field).SetValue(Object, value);
        }

        private MemberAccessor GetFieldAccessor(string name)
        {
            var field = Type.GetRuntimeField(name) ??
                throw new Exception($"Field with name {name} not found");

            return MemberAccessorsCache.Shared.Create(field);
        }

        public object? CallMethod(string name, params object?[] args)
        {
            return CallMethod(name, Type.EmptyTypes, args);
        }

        public object? CallMethod(string name, Type[] genericTypes, params object?[] args)
        {
            return CallMethod(name, genericTypes, args.Select(x => x!.GetType()).ToArray(), args);
        }

        public object? CallMethod(string name, Type[] genericTypes, Type[] argTypes, params object?[] args)
        {
            return GetMethodAccessor(Type, name, genericTypes, argTypes).Invoke(Object, args);
        }

        public object? CallStaticMethod(Type staticType, string name, params object?[] args)
        {
            return CallStaticMethod(staticType, name, Type.EmptyTypes, args);
        }

        public object? CallStaticMethod(Type staticType, string name, Type[] genericTypes, params object?[] args)
        {
            return CallStaticMethod(staticType, name, genericTypes, args.Select(x => x!.GetType()).ToArray(), args);
        }

        public object? CallStaticMethod(Type staticType, string name, Type[] genericTypes, Type[] argTypes, params object?[] args)
        {
            argTypes = argTypes.Prepend(Type).ToArray();
            args = args.Prepend(Object!).ToArray();

            return GetMethodAccessor(staticType, name, genericTypes, argTypes).Invoke(null, args);
        }

        private MemberAccessor GetMethodAccessor(Type type, string name, Type[] genericTypes, Type[] argTypes)
        {
            var method = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.Name == name)
                .Where(x => (int)_generickParameterCountAccessor.GetValue(x)! == genericTypes.Length)
                .Select(x => x.MakeGenericMethod(genericTypes))
                .Where(x => x.GetParameters().Length == argTypes.Length)
                .Where(x => x
                    .GetParameters()
                    .Select((c, i) => (c, i))
                    .All(c => c.c.ParameterType.IsAssignableFrom(argTypes[c.i]))
                )
                .FirstOrDefault() ??
                throw new Exception($"Method with name {name}, generic types {string.Join(", ", genericTypes.Select(x => x.Name))} and arg types {string.Join(", ", argTypes.Select(x => x.Name))} not found");

            return MemberAccessorsCache.Shared.Create(method);
        }

        public static ObjectAccessor Create(Type type, params object?[] args)
        {
            return Create(type, args.Select(x => x!.GetType()).ToArray(), args);
        }

        public static ObjectAccessor Create(Type type, Type[] argTypes, params object?[] args)
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

            var constructorAccessor = MemberAccessorsCache.Shared.Create(constructor);

            var obj = constructorAccessor.Invoke(null, args);

            return new ObjectAccessor(obj, type);
        }

        public static ObjectAccessor CreateStatic(Type type)
        {
            return new ObjectAccessor(null, type);
        }
    }
}
