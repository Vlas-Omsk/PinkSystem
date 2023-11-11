namespace BotsCommon.IO.Data
{
    public abstract class StreamDataReader<T> : IDataReader<T>
    {
        private readonly StreamReader _reader;
        private readonly object _lock = new();
        private int _index;

        public StreamDataReader(StreamReader reader)
        {
            _reader = reader;
            if (reader.BaseStream.CanSeek)
                Length = reader.GetLinesCount();
        }

        public int? Length { get; private set; }
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
            string line;

            lock (_lock)
            {
                line = _reader.ReadLine();

                if (line != null)
                    Index++;
            }

            return ReadOverride(line);
        }

        protected abstract T ReadOverride(string line);

        public void Reset()
        {
            lock (_lock)
            {
                _reader.SetPosition(0);
                Index = 0;
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
