using BotsCommon.Net.Http.Handlers;
using Microsoft.Extensions.Logging;
using System;

namespace BotsCommon.Net.Http.Handlers.Factories
{
    public sealed class SharedPooledHttpRequestHandlerFactory : IHttpRequestHandlerFactory, IDisposable
    {
        private readonly IHttpRequestHandlerWrapper _httpRequestHandlerWrapper;
        private readonly SharedPooledHttpRequestHandler.Pool _pool;

        public SharedPooledHttpRequestHandlerFactory(
            ISocketsHttpRequestHandlerFactory httpRequestHandlerFactory,
            IHttpRequestHandlerWrapper httpRequestHandlerWrapper,
            ILoggerFactory loggerFactory
        )
        {
            _httpRequestHandlerWrapper = httpRequestHandlerWrapper;
            _pool = new SharedPooledHttpRequestHandler.Pool(
                new SharedPooledHttpRequestHandler.PoolConnections(
                    httpRequestHandlerFactory,
                    loggerFactory.CreateLogger<SharedPooledHttpRequestHandler.PoolConnections>()
                )
            );
        }

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            IHttpRequestHandler handler = new SharedPooledHttpRequestHandler(_pool, options);

            return _httpRequestHandlerWrapper.Wrap(handler);
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
