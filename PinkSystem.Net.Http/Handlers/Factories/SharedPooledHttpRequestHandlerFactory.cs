using PinkSystem.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public sealed class SharedPooledHttpRequestHandlerFactory : ISocketsHttpRequestHandlerFactory, IDisposable
    {
        private readonly ISocketsHttpRequestHandlerFactory _httpRequestHandlerFactory;
        private readonly SharedPooledHttpRequestHandler.PoolConnections _poolConnections;
        private readonly SharedPooledHttpRequestHandler.Pool _pool;

        public SharedPooledHttpRequestHandlerFactory(
            ISocketsHttpRequestHandlerFactory httpRequestHandlerFactory,
            ILoggerFactory loggerFactory
        )
        {
            _poolConnections = new SharedPooledHttpRequestHandler.PoolConnections(
                httpRequestHandlerFactory,
                loggerFactory.CreateLogger<SharedPooledHttpRequestHandler.PoolConnections>()
            );
            _pool = new SharedPooledHttpRequestHandler.Pool(
                _poolConnections
            );
            _httpRequestHandlerFactory = httpRequestHandlerFactory;
        }

        public int HandlersInUseAmount => _poolConnections.InUseAmount;
        public int HandlersAmount => _poolConnections.Amount;
        public ISocketsProvider SocketsProvider => _httpRequestHandlerFactory.SocketsProvider;

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            IHttpRequestHandler handler = new SharedPooledHttpRequestHandler(_pool, options);

            return handler;
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
