using System.Collections.Concurrent;
using System.Reflection;

namespace BotsCommon.Runtime
{
    public sealed class MemberAccessorsCache
    {
        private readonly ConcurrentDictionary<MemberInfo, MemberAccessor> _cache = new();

        public static MemberAccessorsCache Shared { get; } = new();

        public MemberAccessor Create(MemberInfo memberInfo)
        {
            return _cache.GetOrAdd(memberInfo, x => new MemberAccessor(x));
        }
    }
}
