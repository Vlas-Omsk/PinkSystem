using Microsoft.Extensions.Logging;
using System;

namespace BotsCommon.Net.Http.Handlers.Factories
{
    public sealed class PooledHttpRequestHandlerFactory : IHttpRequestHandlerFactory, IDisposable
    {
        private readonly IHttpRequestHandlerWrapper _httpRequestHandlerWrapper;
        private readonly PooledHttpRequestHandler.Pool _pool;

        public PooledHttpRequestHandlerFactory(
            ISocketsHttpRequestHandlerFactory httpRequestHandlerFactory,
            IHttpRequestHandlerWrapper httpRequestHandlerWrapper,
            ILoggerFactory loggerFactory
        )
        {
            _httpRequestHandlerWrapper = httpRequestHandlerWrapper;
            _pool = new PooledHttpRequestHandler.Pool(
                new PooledHttpRequestHandler.PoolConnections(
                    httpRequestHandlerFactory,
                    loggerFactory.CreateLogger<PooledHttpRequestHandler.PoolConnections>()
                )
            );
        }

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
