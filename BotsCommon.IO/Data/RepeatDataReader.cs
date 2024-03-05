namespace BotsCommon.IO.Data
{
    public sealed class RepeatDataReader<T> : IDataReader<T>
    {
        private readonly IDataReader<T> _reader;
        private readonly object _lock = new();

        public RepeatDataReader(IDataReader<T> reader)
        {
            _reader = reader;
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public T? Read()
        {
            lock (_lock)
            {
                var data = _reader.Read();

                if (data == null)
                {
                    _reader.Reset();

                    data = _reader.Read();
                }

                return data;
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
