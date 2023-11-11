namespace BotsCommon.IO.Data
{
    public sealed class EnumerableDataReader<T> : IDataReader<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly object _lock = new();
        private int _index;

        public EnumerableDataReader(IEnumerable<T> enumerable) : this(enumerable, null)
        {
        }

        public EnumerableDataReader(IEnumerable<T> enumerable, int? length)
        {
            _enumerator = enumerable.GetEnumerator();
            Length = length;
        }

        ~EnumerableDataReader()
        {
            Dispose();
        }

        public int? Length { get; }
        public int Index
        {
            get
            {
                lock (_lock)
                    return _index;
            }
            set
            {
                lock (_lock)
                    _index = value;
            }
        }

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
            {
                Index = 0;
                _enumerator.Reset();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            lock (_lock)
                _enumerator.Dispose();
        }
    }
}
