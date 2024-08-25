using System.IO;

namespace PinkSystem.IO.Data
{
    public abstract class StreamDataWriter<T> : IDataWriter<T>
    {
        private readonly TextWriter _writer;
        private readonly object _lock = new object();

        public StreamDataWriter(TextWriter writer)
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
