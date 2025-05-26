using System.Collections.Concurrent;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class SharedPool : IPool
    {
        private readonly PoolConnections _connections;
        private readonly ConcurrentDictionary<PoolSettings, SharedPoolMap> _maps = new();

        public SharedPool(PoolConnections connections)
        {
            _connections = connections;
        }

        public PoolConnections Connections => _connections;

        public IPoolMap GetDefaultMap()
        {
            return GetMap(_connections.DefaultSettings);
        }

        public IPoolMap GetMap(PoolSettings settings)
        {
            return _maps.GetOrAdd(settings, (_) => new SharedPoolMap(settings, _connections));
        }

        public void Dispose()
        {
            _connections.Dispose();
        }
    }
}
