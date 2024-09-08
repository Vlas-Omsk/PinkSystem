using PinkSystem.Net.Http.Handlers;
using PinkSystem.Net.Http.Handlers.Factories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http
{
    public sealed class PooledHttpRequestHandler : IHttpRequestHandler
    {
        // if available sockets more than 20%
        private const double _safeFreePercent = 0.2;
        private readonly PoolConnections _poolConnections;
        private WeakReference<PoolConnection> _poolConnection;
        private readonly object _poolConnectionLock = new();

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

                if (!_connections.TryAdd(connection, true))
                    throw new Exception("Connection not added to pool");

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

                        try
                        {
                            DisposeUnusedItems();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error when disposing unused http request handlers");
                        }

                        try
                        {
                            DisposeUnusedSockets();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error when disposing unused sockets http request handlers");
                        }

                        try
                        {
                            DisposeTimeOutedItems();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error when disposing timeouted http request handlers");
                        }
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
                if (_connections.Count < _factory.SocketsProvider.MaxAvailableSockets * 2)
                    return;

                var amountToDispose = _connections.Count - _factory.SocketsProvider.MaxAvailableSockets;
                var disposedAmount = 0;

                foreach (var (connection, _) in _connections.ToArray().OrderBy(x => x.Key.LastUse))
                {
                    if (TryDisposeItem(connection, ignoreNew: false))
                    {
                        disposedAmount++;

                        if (disposedAmount > amountToDispose)
                            break;
                    }
                }

                if (disposedAmount > 0)
                    _logger.LogInformation("Disposed {amount} unused http request handlers", disposedAmount);
            }

            private void DisposeUnusedSockets()
            {
                if (_factory.SocketsProvider.CurrentAvailableSockets > _factory.SocketsProvider.MaxAvailableSockets * _safeFreePercent)
                    return;

                var amountToDispose = (_factory.SocketsProvider.MaxAvailableSockets * _safeFreePercent) - _factory.SocketsProvider.CurrentAvailableSockets;
                var disposedAmount = 0;

                foreach (var (connection, _) in _connections.ToArray().OrderBy(x => x.Key.LastUse))
                {
                    if (TryDisposeItem(connection, ignoreNew: false))
                    {
                        disposedAmount++;

                        if (disposedAmount > amountToDispose)
                            break;
                    }
                }

                if (disposedAmount > 0)
                    _logger.LogInformation("Disposed {amount} unused sockets http request handlers", disposedAmount);
            }

            private void DisposeTimeOutedItems()
            {
                var disposeTime = DateTime.Now - _timeout;
                var disposedAmount = 0;

                foreach (var (connection, _) in _connections)
                {
                    if (connection.LastUse >= disposeTime)
                        continue;

                    if (TryDisposeItem(connection, ignoreNew: true))
                        disposedAmount++;
                }

                if (disposedAmount > 0)
                    _logger.LogInformation("Disposed {amount} timeouted http request handlers", disposedAmount);
            }

            public bool TryDisposeItem(PoolConnection connection, bool ignoreNew)
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

        public sealed class PoolConnection
        {
            private int _rents = 0;
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

            public int RentsAmount => _rents;

            public bool TryRent()
            {
                _lock.EnterReadLock();

                try
                {
                    if (_disposed)
                        return false;

                    Interlocked.Increment(ref _rents);

                    LastUse = DateTime.Now;
                    New = false;

                    return true;
                }
                finally
                {
                    ExitReadLock();
                }
            }

            public void Return()
            {
                _lock.EnterReadLock();

                try
                {
                    ReturnInternal();
                }
                finally
                {
                    ExitReadLock();
                }
            }

            private void ReturnInternal()
            {
                var rents = Interlocked.Decrement(ref _rents);

                if (rents < 0)
                    throw new InvalidOperationException("Not rented by specified handler");
            }

            public bool TryDispose(bool ignoreNew = false)
            {
                _lock.EnterUpgradeableReadLock();

                try
                {
                    if (_disposed)
                        return true;

                    if (!ignoreNew && New)
                        return false;

                    if (RentsAmount > 0)
                        return false;

                    _lock.EnterWriteLock();

                    try
                    {
                        Handler.Dispose();

                        _disposed = true;

                        return true;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }

            private void ExitReadLock()
            {
                _lock.ExitReadLock();
            }
        }

        public PooledHttpRequestHandler(PoolConnections poolConnections, HttpRequestHandlerOptions options)
        {
            _poolConnections = poolConnections;
            _poolConnection = _poolConnections.CreateNew(options);
            Options = options;
        }

        public HttpRequestHandlerOptions Options { get; }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            var connection = Rent();

            try
            {
                return await connection.Handler.SendAsync(request, cancellationToken);
            }
            finally
            {
                connection.Return();
            }
        }

        public IHttpRequestHandler Clone()
        {
            return new PooledHttpRequestHandler(_poolConnections, Options);
        }

        public void Dispose()
        {
            if (_poolConnection.TryGetTarget(out var item))
                _poolConnections.TryDisposeItem(item, ignoreNew: true);
        }

        private PoolConnection Rent()
        {
            while (true)
            {
                lock (_poolConnectionLock)
                {
                    if (_poolConnection.TryGetTarget(out var item) &&
                        item.TryRent())
                        return item;
                    else
                        _poolConnection = _poolConnections.CreateNew(Options);
                }
            }
        }
    }
}
