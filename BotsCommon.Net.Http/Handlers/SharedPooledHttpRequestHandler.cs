using BotsCommon.Net.Http.Handlers;
using BotsCommon.Net.Http.Handlers.Factories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http
{
    public sealed class SharedPooledHttpRequestHandler : IHttpRequestHandler
    {
        private readonly PoolMap _poolMap;

        public sealed class PoolConnections : IDisposable
        {
            private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(3);
            private readonly ISocketsHttpRequestHandlerFactory _factory;
            private readonly ILogger<PoolConnections> _logger;
            private readonly ConcurrentDictionary<PoolConnection, bool> _connections = new();

            public PoolConnections(ISocketsHttpRequestHandlerFactory factory, ILogger<PoolConnections> logger)
            {
                _factory = factory;
                _logger = logger;

                _ = HandleBackground();
            }

            public WeakReference<PoolConnection> CreateNew(HttpRequestHandlerOptions options)
            {
                var connection = new PoolConnection(
                    _factory.Create(options)
                );

                _connections.AddOrUpdate(connection, true, (_, _) => true);

                return new WeakReference<PoolConnection>(connection);
            }

            private async Task HandleBackground()
            {
                while (true)
                {
                    try
                    {
                        _logger.LogInformation(
                            "Http request handlers pool (In use: {rents}, Cached handlers: {amount}, Connections: {current} / {maximum})",
                            _connections.Sum(x => x.Key.RentsAmount),
                            _connections.Count,
                            _factory.SocketsProvider.MaxAvailableSockets - _factory.SocketsProvider.CurrentAvailableSockets,
                            _factory.SocketsProvider.MaxAvailableSockets
                        );

                        DisposeUnusedItems();
                        DisposeTimeOutedItems();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error when disposing http request handlers");
                    }

                    await Task.Delay(5_000);
                }
            }

            private void DisposeUnusedItems()
            {
                // if available sockets more than 70%
                if (_factory.SocketsProvider.CurrentAvailableSockets > _factory.SocketsProvider.MaxAvailableSockets * 0.7)
                    return;

                var disposedAmount = 0;

                foreach (var (connection, _) in _connections)
                {
                    if (TryDisposeItemInternal(connection, ignoreNew: false))
                        disposedAmount++;
                }

                if (disposedAmount > 0)
                    _logger.LogInformation("Disposed {amount} unused http request handlers", disposedAmount);
            }

            private void DisposeTimeOutedItems()
            {
                var disposeTime = DateTime.Now - _timeout;
                var disposedAmount = 0;

                foreach (var (connection, _) in _connections)
                {
                    if (connection.LastUse >= disposeTime)
                        continue;

                    if (TryDisposeItemInternal(connection, ignoreNew: true))
                        disposedAmount++;
                }

                if (disposedAmount > 0)
                    _logger.LogInformation("Disposed {amount} timeouted http request handlers", disposedAmount);
            }

            private bool TryDisposeItemInternal(PoolConnection connection, bool ignoreNew)
            {
                if (!connection.TryDispose(ignoreNew))
                    return false;

                _connections.TryRemove(connection, out _);

                return true;
            }

            public void Dispose()
            {
                foreach (var (connection, _) in _connections)
                    connection.TryDispose(ignoreNew: true);
            }
        }

        public sealed class PoolMap
        {
            private readonly HttpRequestHandlerOptions _options;
            private readonly PoolConnections _connections;
            private WeakReference<PoolConnection> _sharedRef;
            private readonly ConcurrentDictionary<SharedPooledHttpRequestHandler, WeakReference<PoolConnection>> _map = new();

            public PoolMap(HttpRequestHandlerOptions options, PoolConnections connections)
            {
                _options = options;
                _connections = connections;
                _sharedRef = connections.CreateNew(options);
            }

            public PoolConnection Rent(SharedPooledHttpRequestHandler id)
            {
                if (_map.TryGetValue(id, out var itemRef))
                {
                    if (itemRef.TryGetTarget(out var item) &&
                        item.TryRent(id))
                        return item;

                    _map.TryRemove(id, out _);
                }

                if (_sharedRef.TryGetTarget(out var shared) &&
                    shared.TryRent(id))
                    return shared;

                _sharedRef = _connections.CreateNew(_options);

                if (_sharedRef.TryGetTarget(out shared) &&
                    shared.TryRent(id))
                    return shared;

                throw new Exception("Cannot rent created connection");
            }

            public void BindExclusive(SharedPooledHttpRequestHandler id)
            {
                var connection = _connections.CreateNew(_options);

                _map.AddOrUpdate(
                    id,
                    (_, connection) => connection,
                    (_, result, connection) => connection,
                    connection
                );
            }
        }

        public sealed class Pool : IDisposable
        {
            private readonly PoolConnections _connections;
            private readonly ConcurrentDictionary<HttpRequestHandlerOptions, PoolMap> _maps = new();

            public Pool(PoolConnections connections)
            {
                _connections = connections;
            }

            public PoolMap GetMap(HttpRequestHandlerOptions options)
            {
                return _maps.GetOrAdd(options, (_) => new PoolMap(options, _connections));
            }

            public void Dispose()
            {
                _connections.Dispose();
            }
        }

        public sealed class PoolConnection
        {
            private readonly ConcurrentDictionary<SharedPooledHttpRequestHandler, int> _rents = new();
            private bool _disposed = false;
            private readonly ReaderWriterLockSlim _lock = new();

            public PoolConnection(
                IHttpRequestHandler handler
            )
            {
                Handler = handler;
            }

            public bool New { get; private set; } = true;
            public DateTime LastUse { get; private set; } = DateTime.Now;
            public IHttpRequestHandler Handler { get; }

            public int RentsAmount => _rents.Sum(x => x.Value);

            public bool TryRent(SharedPooledHttpRequestHandler id)
            {
                _lock.EnterReadLock();

                try
                {
                    if (_disposed)
                        return false;

                    _rents.AddOrUpdate(id, 1, (x, c) => c + 1);

                    LastUse = DateTime.Now;
                    New = false;

                    return true;
                }
                finally
                {
                    ExitReadLock();
                }
            }

            public void Return(SharedPooledHttpRequestHandler id)
            {
                _lock.EnterReadLock();

                try
                {
                    ReturnInternal(id);
                }
                finally
                {
                    ExitReadLock();
                }
            }

            private void ReturnInternal(SharedPooledHttpRequestHandler id)
            {
                _rents.AddOrUpdate(
                    id,
                    (_) => throw new InvalidOperationException("Not rented by specified handler"),
                    (x, c) =>
                    {
                        if (c <= 0)
                            throw new InvalidOperationException("Not rented by specified handler");

                        return c - 1;
                    }
                );
            }

            public bool TryDispose(bool ignoreNew = false)
            {
                if (_disposed)
                    return true;

                _lock.EnterWriteLock();

                try
                {
                    if (_disposed)
                        return true;

                    if (!ignoreNew && New)
                        return false;

                    if (RentsAmount > 0)
                        return false;

                    Handler.Dispose();

                    _disposed = true;

                    return true;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            private void ExitReadLock()
            {
                _lock.ExitReadLock();
            }
        }

        public SharedPooledHttpRequestHandler(Pool pool, HttpRequestHandlerOptions options)
        {
            _poolMap = pool.GetMap(options);
            Options = options;
        }

        public HttpRequestHandlerOptions Options { get; }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            var connection = _poolMap.Rent(this);

            try
            {
                return await connection.Handler.SendAsync(request, cancellationToken);
            }
            finally
            {
                connection.Return(this);
            }
        }

        public IHttpRequestHandler Clone()
        {
            _poolMap.BindExclusive(this);

            return this;
        }

        public void Dispose()
        {
        }
    }
}
