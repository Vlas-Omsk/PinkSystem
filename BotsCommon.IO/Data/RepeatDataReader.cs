namespace BotsCommon.IO.Data
{
    public sealed class RepeatDataReader<T> : IDataReader<T>
    {
        private readonly IDataReader<T> _reader;
        private object _lock = new();

        public RepeatDataReader(IDataReader<T> reader)
        {
            _reader = reader;
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public T? Read()
        {
            T? data;

            lock (_lock)
            {
                data = _reader.Read();

                if (data == null)
                {
                    Reset();

                    data = _reader.Read();
                }
            }

            return data;
        }

        public void Reset()
        {
            lock (_lock)
                _reader.Reset();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
