using System;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class PooledHttpRequestHandlerFactory : IHttpRequestHandlerFactory, IDisposable
    {
        private readonly IPool _pool;

        private sealed class PooledHttpRequestHandler : IHttpRequestHandler
        {
            private readonly IPoolMap _poolMap;

            public PooledHttpRequestHandler(IPoolMap poolMap)
            {
                _poolMap = poolMap;
            }

            public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
            {
                var poolConnection = _poolMap.RentConnection(this);

                try
                {
                    return await poolConnection.Handler.SendAsync(request, cancellationToken);
                }
                finally
                {
                    poolConnection.Return(this);
                }
            }

            public void Dispose()
            {
                _poolMap.DisposeConnection(this, ignoreNew: true);
            }
        }

        public PooledHttpRequestHandlerFactory(IPool pool)
        {
            _pool = pool;
        }

        public IHttpRequestHandler Create(IHttpRequestHandlerOptions? options)
        {
            return new PooledHttpRequestHandler(
                _pool.GetMap(options)
            );
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
