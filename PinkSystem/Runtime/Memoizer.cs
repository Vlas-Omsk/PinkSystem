using System;
using System.Collections.Concurrent;

namespace PinkSystem.Runtime
{
    public sealed class Memoizer<TType>
    {
        private readonly ConcurrentDictionary<int, object> _cache = new();

        public static Memoizer<TType> Shared { get; } = new();

        public T GetOrAddMemoizedValue<T>(Func<T> func, params object[] args)
        {
            var hashCode = GetHashCode(args);

            return (T)_cache.GetOrAdd(hashCode, x =>
            {
                var value = func();

                return value == null ? 0 : value;
            });
        }

        private static int GetHashCode(object[] args)
        {
            return args.Length switch
            {
                0 => 0,
                1 => HashCode.Combine(args[0]),
                2 => HashCode.Combine(args[0], args[1]),
                3 => HashCode.Combine(args[0], args[1], args[2]),
                4 => HashCode.Combine(args[0], args[1], args[2], args[3]),
                5 => HashCode.Combine(args[0], args[1], args[2], args[3], args[4]),
                6 => HashCode.Combine(args[0], args[1], args[2], args[3], args[4], args[5]),
                7 => HashCode.Combine(args[0], args[1], args[2], args[3], args[4], args[5], args[6]),
                8 => HashCode.Combine(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]),
                _ => throw new NotSupportedException("More than 8 args not supported")
            };
        }
    }
}
