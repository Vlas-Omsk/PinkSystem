using PinkSystem.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public sealed class PooledHttpRequestHandlerFactory : ISocketsHttpRequestHandlerFactory, IDisposable
    {
        private readonly ISocketsHttpRequestHandlerFactory _httpRequestHandlerFactory;
        private readonly PooledHttpRequestHandler.PoolConnections _pool;

        public PooledHttpRequestHandlerFactory(
            ISocketsHttpRequestHandlerFactory httpRequestHandlerFactory,
            ILoggerFactory loggerFactory
        )
        {
            _httpRequestHandlerFactory = httpRequestHandlerFactory;
            _pool = new PooledHttpRequestHandler.PoolConnections(
                httpRequestHandlerFactory,
                loggerFactory.CreateLogger<PooledHttpRequestHandler.PoolConnections>()
            );
        }

        public ISocketsProvider SocketsProvider => _httpRequestHandlerFactory.SocketsProvider;

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            IHttpRequestHandler handler = new PooledHttpRequestHandler(_pool, options);

            return handler;
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
