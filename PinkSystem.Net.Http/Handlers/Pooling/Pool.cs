using System.Collections.Concurrent;
using PinkSystem.Net.Http.Handlers.Factories;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class Pool : IPool
    {
        private readonly PoolConnections _connections;
        private readonly ConcurrentDictionary<PoolSettings, PoolMap> _maps = new();

        public Pool(PoolConnections connections)
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
            return _maps.GetOrAdd(settings, (_) => new PoolMap(settings, _connections));
        }

        public void Dispose()
        {
            _connections.Dispose();
        }
    }
}
