using System;
using System.Collections.Generic;

namespace PinkSystem.IO.Data
{
    public sealed class EnumerableDataReader<T> : IDataReader<T>
    {
        private readonly Func<int?> _lengthGetter = () => null;
        private readonly IEnumerable<T> _enumerable;
        private IEnumerator<T> _enumerator;
        private readonly object _lock = new();
        private int _index;

        public EnumerableDataReader(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
            _enumerator = enumerable.GetEnumerator();

            if (enumerable is IReadOnlyList<T> list)
            {
                _lengthGetter = () => list.Count;
            }
        }

        public int? Length => _lengthGetter();
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
