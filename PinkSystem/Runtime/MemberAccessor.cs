using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PinkSystem.Runtime
{
    public sealed class MemberAccessor
    {
        private static MethodInfo _unboxPointerMethod = typeof(MemberAccessor)
            .GetMethod(nameof(UnboxPointer), BindingFlags.Static | BindingFlags.NonPublic)!;
        private Func<object?, object?>? _getter;
        private Action<object?, object?>? _setter;
        private Func<object?, object?[], object?>? _invoker;

        public MemberAccessor(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }

        public MemberInfo MemberInfo { get; }

        public object? GetValue(object? obj)
        {
            if (_getter == null)
                CompileGetter();

            return _getter!.Invoke(obj);
        }

        public void SetValue(object? obj, object? value)
        {
            if (_setter == null)
                CompileSetter();

            _setter!.Invoke(obj, value);
        }

        public object? Invoke(object? obj, params object?[] args)
        {
            if (_invoker == null)
                CompileInvoker();

            return _invoker!.Invoke(obj, args);
        }

        private void CompileGetter()
        {
            var targetType = MemberInfo.DeclaringType!;

            var exInstance = Expression.Parameter(typeof(object), "instance");

            var exConvertedInstance = Expression.Convert(exInstance, targetType);
            var exMemberAccess = Expression.MakeMemberAccess(IsStatic(MemberInfo) ? null : exConvertedInstance, MemberInfo);
            var exConvertedResult = Expression.Convert(exMemberAccess, typeof(object));

            var lambda = Expression.Lambda<Func<object?, object?>>(exConvertedResult, exInstance);

            _getter = lambda.Compile();
        }

        private void CompileSetter()
        {
            var targetType = MemberInfo.DeclaringType!;

            var exInstance = Expression.Parameter(typeof(object), "instance");
            var exValue = Expression.Parameter(typeof(object), "value");

            var exConvertedInstance = Expression.Convert(exInstance, targetType);
            var exMemberAccess = Expression.MakeMemberAccess(IsStatic(MemberInfo) ? null : exConvertedInstance, MemberInfo);
            var exConvertedValue = Expression.Convert(exValue, GetUnderlyingType(MemberInfo));
            var exAssign = Expression.Assign(exMemberAccess, exConvertedValue);

            var lambda = Expression.Lambda<Action<object?, object?>>(exAssign, exInstance, exValue);

            _setter = lambda.Compile();
        }

        private void CompileInvoker()
        {
            var targetType = MemberInfo.DeclaringType!;

            var exInstance = Expression.Parameter(typeof(object), "instance");
            var exParameters = Expression.Variable(typeof(object[]), "parameters");

            var exConvertedInstance = Expression.Convert(exInstance, targetType);

            if (!(MemberInfo is MethodBase methodBase))
                throw new InvalidOperationException($"{nameof(MemberInfo)} must be of type {nameof(MethodBase)}");

            var parameters = methodBase.GetParameters();

            var exParametersArray = new Expression[parameters.Length];
            var exVariablesArray = new List<ParameterExpression>(parameters.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;

                if (parameterType.IsByRef)
                {
                    var exVariable = Expression.Variable(parameterType.GetElementType()!, parameters[i].Name);

                    exVariablesArray.Add(exVariable);
                    exParametersArray[i] = exVariable;
                }
                else
                {
                    var exIndex = Expression.Constant(i);
                    var exParameter = Expression.ArrayIndex(exParameters, exIndex);

                    if (parameterType.IsPointer)
                    {
                        exParametersArray[i] = Expression.Call(
                            null,
                            _unboxPointerMethod.MakeGenericMethod(parameterType.GetElementType()!),
                            [
                                exParameter
                            ]
                        );
                    }
                    else
                    {
                        var exConvertedParameter = Expression.Convert(exParameter, parameterType);

                        exParametersArray[i] = exConvertedParameter;
                    }
                }
            }

            Expression exResult;

            IEnumerable<Expression> exArrayItems = exVariablesArray
                .Select(x =>
                    Expression.Convert(x, typeof(object))
                );

            if (MemberInfo is ConstructorInfo constructor)
            {
                exResult = Expression.New(constructor, exParametersArray);

                exArrayItems = exArrayItems.Prepend(
                    Expression.Convert(exResult, typeof(object))
                );
            }
            else if (MemberInfo is MethodInfo method)
            {
                exResult = Expression.Call(methodBase.IsStatic ? null : exConvertedInstance, method, exParametersArray);

                if (method.ReturnType == typeof(void))
                {
                    exArrayItems = exArrayItems.Prepend(
                        Expression.Constant(null)
                    );
                }
                else
                {
                    exArrayItems = exArrayItems.Prepend(
                        Expression.Convert(exResult, typeof(object))
                    );
                }
            }
            else
            {
                throw new InvalidOperationException($"{nameof(MemberInfo)} must be of type {nameof(ConstructorInfo)} or {nameof(MethodInfo)}");
            }

            exResult = Expression.Block(
                exVariablesArray,
                Expression.NewArrayInit(
                    typeof(object),
                    exArrayItems
                )
            );

            var lambda = Expression.Lambda<Func<object?, object?[], object?[]>>(exResult, exInstance, exParameters);

            var func = lambda.Compile();

            _invoker = (instance, args) =>
            {
                var result = func.Invoke(instance, args);

                var refIndex = 1;

                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    if (parameter.ParameterType.IsByRef)
                        args[i] = result[refIndex++];
                }

                return result[0];
            };
        }

        public static Type GetUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType!;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException($"{nameof(MemberInfo)} must be of type {nameof(EventInfo)}, {nameof(FieldInfo)}, {nameof(MethodInfo)} or {nameof(PropertyInfo)}");
            }
        }

        public static bool IsStatic(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).IsStatic;
                case MemberTypes.Method:
                    return ((MethodInfo)member).IsStatic;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).GetAccessors().Any(x => x.IsStatic);
                default:
                    throw new ArgumentException($"{nameof(MemberInfo)} must be of type {nameof(FieldInfo)}, {nameof(MethodInfo)} or {nameof(PropertyInfo)}");
            }
        }

        private unsafe static T* UnboxPointer<T>(object pointer)
            where T : unmanaged
        {
            return (T*)Pointer.Unbox(pointer);
        }
    }
}
