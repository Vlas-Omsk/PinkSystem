using System;
using System.Collections.Generic;

namespace BotsCommon.IO.Data
{
    public sealed class RandomDataReader<T> : IDataReader<T>
    {
        private readonly IDataReader<T> _reader;
        private readonly object _lock = new();
        private readonly List<T?> _items = new();

        public RandomDataReader(IDataReader<T> dataReader)
        {
            _reader = dataReader;
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public T? Read()
        {
            if (!_reader.Length.HasValue)
                throw new Exception("Length must be non null");

            var skipCount = Random.Shared.Next(0, (_reader.Length.Value - _reader.Index) + _items.Count);

            lock (_lock)
            {
                if (skipCount < _items.Count)
                {
                    var item = _items[skipCount];

                    _items.RemoveAt(skipCount);

                    return item;
                }

                for (var i = 0; i < skipCount - _items.Count; i++)
                {
                    var item = _reader.Read();

                    _items.Add(item);
                }

                return _reader.Read();
            }
        }

        public void Reset()
        {
            lock (_lock)
                _reader.Reset();
        }

        public void Dispose()
        {
            lock (_lock)
                _reader.Dispose();
        }
    }
}
