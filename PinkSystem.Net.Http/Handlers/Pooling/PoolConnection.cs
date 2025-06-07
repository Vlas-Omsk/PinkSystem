using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class PoolConnection
    {
        private readonly ConcurrentDictionary<IHttpRequestHandler, int> _rents = new();
        private bool _disposed = false;
        private readonly ReaderWriterLockSlim _lock = new();
        private bool _new = true;

        public PoolConnection(
            IHttpRequestHandler handler
        )
        {
            Handler = handler;
        }

        public IHttpRequestHandler Handler { get; }
        public DateTime LastRent { get; private set; } = DateTime.Now;

        public int CurrentRentsAmount => _rents.Sum(x => x.Value);

        public bool TryRent(IHttpRequestHandler handler)
        {
            _lock.EnterReadLock();

            try
            {
                if (_disposed)
                    return false;

                _rents.AddOrUpdate(handler, 1, (x, c) => c + 1);

                LastRent = DateTime.Now;

                _new = false;

                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Return(IHttpRequestHandler handler)
        {
            _lock.EnterReadLock();

            try
            {
                ReturnInternal(handler);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private void ReturnInternal(IHttpRequestHandler handler)
        {
            _rents.AddOrUpdate(
                handler,
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

            _lock.EnterUpgradeableReadLock();

            try
            {
                if (_disposed)
                    return true;

                if (!ignoreNew && _new)
                    return false;

                if (CurrentRentsAmount > 0)
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
    }
}
