namespace BotsCommon.IO.Data
{
    public sealed class UsageLimitedDataReader<T> : IDataReader<T>
    {
        private readonly int _maximumNumberOfUsage;
        private readonly IDataReader<T> _reader;
        private readonly IUsageLimiter<T> _limiter;
        private readonly object _lock = new();

        public UsageLimitedDataReader(
            int maximumNumberOfUsage,
            IDataReader<T> reader,
            IUsageLimiter<T> limiter
        )
        {
            _maximumNumberOfUsage = maximumNumberOfUsage;
            _reader = reader;
            _limiter = limiter;
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public T? Read()
        {
            if (_maximumNumberOfUsage == 0)
                return _reader.Read();

            lock (_lock)
            {
                if (TryRead(out var item))
                    return item;

                Reset();

                if (TryRead(out item))
                    return item;

                throw new LimitReachedException();
            }
        }

        public void Reset()
        {
            _reader.Reset();
        }

        private bool TryRead(out T? item)
        {
            while ((item = _reader.Read()) != null)
                if (CanUse(item))
                    return true;

            return false;
        }

        private bool CanUse(T item)
        {
            var numberOfUses = _limiter.GetNumberOfUses(item);

            if (numberOfUses >= _maximumNumberOfUsage)
                return false;

            _limiter.IncreaseNumberOfUses(item);

            return true;
        }

        public void Dispose()
        {
            _reader.Dispose();
            _limiter.Dispose();
        }
    }
}
