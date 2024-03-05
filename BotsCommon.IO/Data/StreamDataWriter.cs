using System.IO;

namespace BotsCommon.IO.Data
{
    public abstract class StreamDataWriter<T> : IDataWriter<T>
    {
        private readonly StreamWriter _writer;
        private readonly object _lock = new object();

        public StreamDataWriter(StreamWriter writer)
        {
            _writer = writer;
        }

        public void Write(T? data)
        {
            lock (_lock)
            {
                _writer.WriteLine(WriteOverride(data));
            }
        }

        protected abstract string? WriteOverride(T? data);

        public void Flush()
        {
            lock (_lock)
            {
                _writer.Flush();
            }
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
