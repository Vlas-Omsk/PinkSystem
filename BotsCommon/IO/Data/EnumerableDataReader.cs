namespace BotsCommon.IO.Data
{
    public sealed class EnumerableDataReader<T> : IDataReader<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly object _lock = new object();

        public EnumerableDataReader(IEnumerable<T> enumerable) : this(enumerable, 0)
        {
        }

        public EnumerableDataReader(IEnumerable<T> enumerable, int length)
        {
            _enumerator = enumerable.GetEnumerator();
            Length = length;
        }

        ~EnumerableDataReader()
        {
            Dispose();
        }

        public int Length { get; }
        public int Index { get; private set; }

        public T Read()
        {
            lock (_lock)
            {
                if (_enumerator.MoveNext())
                {
                    Index++;
                    return _enumerator.Current;
                }
            }

            return default;
        }

        public void Reset()
        {
            lock (_lock)
                _enumerator.Reset();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            lock (_lock)
                _enumerator.Dispose();
        }
    }
}
