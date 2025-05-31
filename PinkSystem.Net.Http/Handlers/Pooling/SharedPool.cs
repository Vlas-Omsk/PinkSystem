using System.Collections.Concurrent;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class SharedPool : IPool
    {
        private readonly PoolConnections _connections;
        private readonly SharedPoolMap _defaultMap;
        private readonly ConcurrentDictionary<IHttpRequestHandlerOptions, SharedPoolMap> _maps = new();

        public SharedPool(PoolConnections connections)
        {
            _defaultMap = new SharedPoolMap(options: null, connections);
            _connections = connections;
        }

        public IPoolMap GetMap(IHttpRequestHandlerOptions? options)
        {
            if (options == null)
                return _defaultMap;

            return _maps.GetOrAdd(options, (_) => new SharedPoolMap(options, _connections));
        }

        public void Dispose()
        {
            _connections.Dispose();
        }
    }
}
