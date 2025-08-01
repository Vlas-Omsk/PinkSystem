﻿using System;
using System.Collections.Generic;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    internal sealed class PoolMap : IPoolMap
    {
        private readonly IHttpRequestHandlerOptions? _options;
        private readonly PoolConnections _connections;
        private readonly Dictionary<IHttpRequestHandler, WeakReference<PoolConnection>> _map = new();
        private readonly object _mapLock = new();

        public PoolMap(IHttpRequestHandlerOptions? options, PoolConnections connections)
        {
            _options = options;
            _connections = connections;
        }

        public PoolConnection RentConnection(IHttpRequestHandler handler)
        {
            for (var i = 0; i < 2; i++)
            {
                lock (_mapLock)
                {
                    PoolConnection? connection;

                    if (!_map.TryGetValue(handler, out var connectionRef))
                    {
                        connectionRef = _connections.CreateNew(_options);

                        _map.Add(handler, connectionRef);
                    }

                    if (connectionRef.TryGetTarget(out connection) &&
                        connection.TryRent(handler))
                        return connection;

                    _map.Remove(handler);
                }
            }

            throw new Exception("Cannot rent connection");
        }

        public void BindExclusiveConnection(IHttpRequestHandler handler)
        {
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
