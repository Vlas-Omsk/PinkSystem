using System.Collections.Concurrent;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class Pool : IPool
    {
        private readonly PoolConnections _connections;
        private readonly PoolMap _defaultMap;
        private readonly ConcurrentDictionary<IHttpRequestHandlerOptions, PoolMap> _maps = new();

        public Pool(PoolConnections connections)
        {
            _defaultMap = new PoolMap(options: null, connections);
            _connections = connections;
        }

        public IPoolMap GetMap(IHttpRequestHandlerOptions? options)
        {
            if (options == null)
                return _defaultMap;

            return _maps.GetOrAdd(options, (_) => new PoolMap(options, _connections));
        }

        public void Dispose()
        {
            _connections.Dispose();
        }
    }
}
