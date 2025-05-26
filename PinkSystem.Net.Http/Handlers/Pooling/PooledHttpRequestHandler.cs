using System;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class PooledHttpRequestHandler : IHttpRequestHandler
    {
        private readonly IPool _pool;
        private IPoolMap _poolMap;

        public PooledHttpRequestHandler(IPool pool)
        {
            _pool = pool;
            _poolMap = pool.GetDefaultMap();
        }

        public Proxy? Proxy
        {
            get => _poolMap.Settings.Proxy;
            set
            {
                var settings = new PoolSettings()
                {
                    Proxy = value,
                    ValidateSsl = ValidateSsl,
                    Timeout = Timeout
                };

                _poolMap = _pool.GetMap(settings);
            }
        }

        public bool ValidateSsl
        {
            get => _poolMap.Settings.ValidateSsl;
            set
            {
                var settings = new PoolSettings()
                {
                    Proxy = Proxy,
                    ValidateSsl = value,
                    Timeout = Timeout
                };

                _poolMap = _pool.GetMap(settings);
            }
        }

        public TimeSpan Timeout
        {
            get => _poolMap.Settings.Timeout;
            set
            {
                var settings = new PoolSettings()
                {
                    Proxy = Proxy,
                    ValidateSsl = ValidateSsl,
                    Timeout = value
                };

                _poolMap = _pool.GetMap(settings);
            }
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

        public IHttpRequestHandler Clone()
        {
            var handler = new PooledHttpRequestHandler(_pool);

            this.CopySettingsTo(handler);

            _poolMap.BindExclusiveConnection(handler);

            return handler;
        }

        public void Dispose()
        {
            _poolMap.DisposeConnection(this, ignoreNew: true);
        }
    }
}
