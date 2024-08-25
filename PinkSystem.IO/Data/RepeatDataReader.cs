namespace PinkSystem.IO.Data
{
    public class RepeatDataReader : IDataReader
    {
        private readonly IDataReader _reader;
        private readonly object _lock = new();

        public RepeatDataReader(IDataReader reader)
        {
            _reader = reader;
        }

        public int? Length { get; } = null;
        public int Index => _reader.Index;

        public object? Read()
        {
            object? data;

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

    public sealed class RepeatDataReader<T> : RepeatDataReader, IDataReader<T>
    {
        public RepeatDataReader(IDataReader<T> reader) : base(reader)
        {
        }

        public new T? Read()
        {
            return (T?)base.Read();
        }
    }
}
