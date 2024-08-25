using System.Collections.Generic;
using System.Linq;

namespace PinkSystem.IO.Data
{
    public sealed class EnumerableDataReader<T> : IDataReader<T>
    {
        private readonly IEnumerable<T> _enumerable;
        private IEnumerator<T> _enumerator;
        private readonly object _lock = new();
        private int _index;

        public EnumerableDataReader(IEnumerable<T> enumerable) : this(enumerable, enumerable.Count())
        {
        }

        public EnumerableDataReader(IEnumerable<T> enumerable, int? length)
        {
            _enumerable = enumerable;
            _enumerator = enumerable.GetEnumerator();
            Length = length;
        }

        public int? Length { get; }
        public int Index => _index;

        public T? Read()
        {
            lock (_lock)
            {
                if (_enumerator.MoveNext())
                {
                    _index++;
                    return _enumerator.Current;
                }
            }

            return default;
        }

        object? IDataReader.Read()
        {
            return Read();
        }

        public void Reset()
        {
            lock (_lock)
            {
                _index = 0;
                _enumerator.Dispose();
                _enumerator = _enumerable.GetEnumerator();
            }
        }

        public void Dispose()
        {
            lock (_lock)
                _enumerator.Dispose();
        }
    }
}
