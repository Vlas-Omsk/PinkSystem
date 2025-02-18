using PinkSystem.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public sealed class SharedPooledHttpRequestHandlerFactory : ISocketsHttpRequestHandlerFactory, IDisposable
    {
        private readonly SharedPooledHttpRequestHandler.Pool _pool;

        public SharedPooledHttpRequestHandlerFactory(
            ISocketsHttpRequestHandlerFactory httpRequestHandlerFactory,
            ILoggerFactory loggerFactory
        )
        {
            _pool = new SharedPooledHttpRequestHandler.Pool(
                new SharedPooledHttpRequestHandler.PoolConnections(
                    httpRequestHandlerFactory,
                    loggerFactory.CreateLogger<SharedPooledHttpRequestHandler.PoolConnections>()
                )
            );
            SocketsProvider = httpRequestHandlerFactory.SocketsProvider;
        }

        public ISocketsProvider SocketsProvider { get; }

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
