using PinkSystem.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public sealed class PooledHttpRequestHandlerFactory : ISocketsHttpRequestHandlerFactory, IDisposable
    {
        private readonly ISocketsHttpRequestHandlerFactory _httpRequestHandlerFactory;
        private readonly IHttpRequestHandlerWrapper _httpRequestHandlerWrapper;
        private readonly PooledHttpRequestHandler.PoolConnections _pool;

        public PooledHttpRequestHandlerFactory(
            ISocketsHttpRequestHandlerFactory httpRequestHandlerFactory,
            IHttpRequestHandlerWrapper httpRequestHandlerWrapper,
            ILoggerFactory loggerFactory
        )
        {
            _httpRequestHandlerFactory = httpRequestHandlerFactory;
            _httpRequestHandlerWrapper = httpRequestHandlerWrapper;
            _pool = new PooledHttpRequestHandler.PoolConnections(
                httpRequestHandlerFactory,
                loggerFactory.CreateLogger<PooledHttpRequestHandler.PoolConnections>()
            );
        }

        public ISocketsProvider SocketsProvider => _httpRequestHandlerFactory.SocketsProvider;

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            IHttpRequestHandler handler = new PooledHttpRequestHandler(_pool, options);

            return _httpRequestHandlerWrapper.Wrap(handler);
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
