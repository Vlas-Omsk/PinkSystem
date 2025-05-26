using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PinkSystem.Net.Http.Handlers.Factories;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class PoolConnections : IDisposable
    {
        // if available sockets more than 20%
        private const double _safeFreePercent = 0.2;
        private static readonly TimeSpan _disposeTimeout = TimeSpan.FromMinutes(3);
        private readonly ILogger<PoolConnections> _logger;
        private readonly ConcurrentDictionary<PoolConnection, bool> _connections = new();

        public PoolConnections(ISocketsHttpRequestHandlerFactory factory, ILogger<PoolConnections> logger)
        {
            HttpRequestHandlerFactory = factory;
            _logger = logger;

            using var handler = HttpRequestHandlerFactory.Create();

            DefaultSettings = new()
            {
                Proxy = handler.Proxy,
                ValidateSsl = handler.ValidateSsl,
                Timeout = handler.Timeout,
            };

            _ = HandleBackground();
        }

        public ISocketsHttpRequestHandlerFactory HttpRequestHandlerFactory { get; }

        public int InUseAmount => _connections.Where(x => x.Key.CurrentRentsAmount > 0).Sum(x => 1);
        public int Amount => _connections.Count;

        internal PoolSettings DefaultSettings { get; }

        internal WeakReference<PoolConnection> CreateNew()
        {
            var handler = HttpRequestHandlerFactory.Create();
            var connection = new PoolConnection(handler);

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
            if (_connections.Count < HttpRequestHandlerFactory.SocketsProvider.MaxAvailableSockets * 2)
                return;

            var amountToDispose = _connections.Count - HttpRequestHandlerFactory.SocketsProvider.MaxAvailableSockets;
            var disposedAmount = 0;

            foreach (var (connection, _) in _connections.ToArray().OrderBy(x => x.Key.LastRent))
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
            if (HttpRequestHandlerFactory.SocketsProvider.CurrentAvailableSockets > HttpRequestHandlerFactory.SocketsProvider.MaxAvailableSockets * _safeFreePercent)
                return;

            var amountToDispose = HttpRequestHandlerFactory.SocketsProvider.MaxAvailableSockets * _safeFreePercent - HttpRequestHandlerFactory.SocketsProvider.CurrentAvailableSockets;
            var disposedAmount = 0;

            foreach (var (connection, _) in _connections.ToArray().OrderBy(x => x.Key.LastRent))
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
            var disposeTime = DateTime.Now - _disposeTimeout;
            var disposedAmount = 0;

            foreach (var (connection, _) in _connections)
            {
                if (connection.LastRent >= disposeTime)
                    continue;

                if (TryDisposeItem(connection, ignoreNew: true))
                    disposedAmount++;
            }

            if (disposedAmount > 0)
                _logger.LogInformation("Disposed {amount} timeouted http request handlers", disposedAmount);
        }

        internal bool TryDisposeItem(PoolConnection connection, bool ignoreNew)
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
}
