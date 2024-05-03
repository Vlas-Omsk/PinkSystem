using System;
using System.Collections.Generic;

namespace BotsCommon.IO.Data
{
    public sealed class RandomDataReader<T> : IDataReader<T>
    {
        private readonly IDataReader<T> _reader;
        private readonly object _lock = new();
        private readonly List<T?> _items;
        private readonly int _maxRange;
        private readonly int _maxBufferSize;

        public RandomDataReader(IDataReader<T> dataReader, int maxRange = int.MaxValue)
        {
            _reader = dataReader;
            _maxRange = maxRange;
            _maxBufferSize = (int)Math.Min(_maxRange * 2L, int.MaxValue);
            _items = new(_maxBufferSize);
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public T? Read()
        {
            if (!_reader.Length.HasValue)
                throw new Exception("Length must be non null");

            lock (_lock)
            {
                if (_items.Count >= _maxBufferSize)
                {
                    var index = Random.Shared.Next(0, _items.Count);
                    var item = _items[index];

                    _items.RemoveAt(index);

                    return item;
                }
            }

            var skipCount = Random.Shared.Next(0, Math.Min(_reader.Length.Value - _reader.Index, _maxRange) + _items.Count);

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
