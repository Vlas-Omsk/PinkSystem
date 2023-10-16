namespace BotsCommon.IO
{
    public abstract class StreamDataWriter<T> : IDataWriter<T>
    {
        private readonly StreamWriter _writer;
        private readonly object _lock = new object();

        public StreamDataWriter(StreamWriter writer)
        {
            _writer = writer;
        }

        ~StreamDataWriter()
        {
            Dispose();
        }

        public void Write(T data)
        {
            lock (_lock)
            {
                _writer.WriteLine(WriteOverride(data));
            }
        }

        protected abstract string WriteOverride(T data);

        public void Flush()
        {
            lock (_lock)
            {
                _writer.Flush();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _writer?.Dispose();
        }
    }
}
