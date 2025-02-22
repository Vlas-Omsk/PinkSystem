using PinkSystem.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public sealed class PooledHttpRequestHandlerFactory : ISocketsHttpRequestHandlerFactory, IDisposable
    {
        private readonly ISocketsHttpRequestHandlerFactory _httpRequestHandlerFactory;
        private readonly PooledHttpRequestHandler.PoolConnections _poolConnections;

        public PooledHttpRequestHandlerFactory(
            ISocketsHttpRequestHandlerFactory httpRequestHandlerFactory,
            ILoggerFactory loggerFactory
        )
        {
            _httpRequestHandlerFactory = httpRequestHandlerFactory;
            _poolConnections = new PooledHttpRequestHandler.PoolConnections(
                httpRequestHandlerFactory,
                loggerFactory.CreateLogger<PooledHttpRequestHandler.PoolConnections>()
            );
        }

        public int HandlersInUseAmount => _poolConnections.InUseAmount;
        public int HandlersAmount => _poolConnections.Amount;
        public ISocketsProvider SocketsProvider => _httpRequestHandlerFactory.SocketsProvider;

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            IHttpRequestHandler handler = new PooledHttpRequestHandler(_poolConnections, options);

            return handler;
        }

        public void Dispose()
        {
            _poolConnections.Dispose();
        }
    }
}
