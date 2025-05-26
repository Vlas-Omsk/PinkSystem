using System;
using System.Collections.Generic;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class SharedPoolMap : IPoolMap
    {
        private readonly PoolConnections _connections;
        private WeakReference<PoolConnection> _sharedConnectionRef;
        private readonly object _sharedConnectionRefLock = new();
        private readonly Dictionary<IHttpRequestHandler, WeakReference<PoolConnection>> _map = new();
        private readonly object _mapLock = new();

        public SharedPoolMap(PoolSettings settings, PoolConnections connections)
        {
            Settings = settings;

            _connections = connections;
            _sharedConnectionRef = _connections.CreateNew();

            if (_sharedConnectionRef.TryGetTarget(out var sharedConnection))
                Settings.ApplyTo(sharedConnection.Handler);
        }

        public PoolSettings Settings { get; }

        public PoolConnection RentConnection(IHttpRequestHandler handler)
        {
            lock (_mapLock)
            {
                if (_map.TryGetValue(handler, out var connectionRef))
                {
                    if (connectionRef.TryGetTarget(out var item) &&
                        item.TryRent(handler))
                        return item;

                    _map.Remove(handler);
                }
            }

            lock (_sharedConnectionRefLock)
            {
                if (_sharedConnectionRef.TryGetTarget(out var sharedConnection) &&
                    sharedConnection.TryRent(handler))
                    return sharedConnection;

                _sharedConnectionRef = _connections.CreateNew();

                if (_sharedConnectionRef.TryGetTarget(out sharedConnection))
                    Settings.ApplyTo(sharedConnection.Handler);

                if (_sharedConnectionRef.TryGetTarget(out sharedConnection) &&
                    sharedConnection.TryRent(handler))
                    return sharedConnection;
            }

            throw new Exception("Cannot rent connection");
        }

        public void BindExclusiveConnection(IHttpRequestHandler handler)
        {
            lock (_mapLock)
            {
                if (_map.TryGetValue(handler, out _))
                    return;

                var connectionRef = _connections.CreateNew();

                if (connectionRef.TryGetTarget(out var connection))
                    Settings.ApplyTo(connection.Handler);

                _map.Add(handler, connectionRef);
            }
        }

        public void DisposeConnection(IHttpRequestHandler handler, bool ignoreNew)
        {
            lock (_mapLock)
            {
                if (_map.TryGetValue(handler, out var connectionRef))
                {
                    if (connectionRef.TryGetTarget(out var connection) &&
                        connection.TryDispose(ignoreNew))
                        _map.Remove(handler);
                }
            }
        }
    }
}
