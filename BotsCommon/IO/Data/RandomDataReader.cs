namespace BotsCommon.IO.Data
{
    public sealed class RandomDataReader<T> : IDataReader<T>
    {
        private readonly IDataReader<T> _reader;
        private readonly object _lock = new();

        public RandomDataReader(IDataReader<T> dataReader)
        {
            _reader = dataReader;
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public T Read()
        {
            if (!_reader.Length.HasValue)
                throw new Exception("Length must be non null");

            lock (_lock)
            {
                var skipCount = Random.Shared.Next(0, _reader.Length.Value - _reader.Index);

                for (var i = 0; i < skipCount; i++)
                    _reader.Read();

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
