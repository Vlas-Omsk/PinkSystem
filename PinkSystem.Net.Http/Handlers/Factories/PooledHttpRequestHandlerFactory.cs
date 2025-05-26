using PinkSystem.Net.Sockets;
using System;
using PinkSystem.Net.Http.Handlers.Pooling;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public sealed class PooledHttpRequestHandlerFactory : ISocketsHttpRequestHandlerFactory, IDisposable
    {
        private readonly IPool _pool;

        public PooledHttpRequestHandlerFactory(IPool pool)
        {
            _pool = pool;
        }

        public ISocketsProvider SocketsProvider => _pool.Connections.HttpRequestHandlerFactory.SocketsProvider;

        public IHttpRequestHandler Create()
        {
            var handler = new PooledHttpRequestHandler(_pool);

            return handler;
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
