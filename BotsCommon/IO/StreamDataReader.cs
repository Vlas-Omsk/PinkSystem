using System;

namespace BotsCommon.IO
{
    public abstract class StreamDataReader<T> : IDataReader<T>, IDisposable
    {
        private readonly StreamReader _reader;
        private readonly object _lock = new object();

        public StreamDataReader(StreamReader reader)
        {
            _reader = reader;
            if (reader.BaseStream.CanSeek)
                Length = reader.GetLinesCount();
        }

        ~StreamDataReader()
        {
            Dispose();
        }

        public int Length { get; private set; }
        public int Index { get; private set; }
        public bool ResetOnEnd { get; set; }

        public T Read()
        {
            string line;

            lock (_lock)
            {
                if (ResetOnEnd && _reader.EndOfStream)
                    Reset();

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
            GC.SuppressFinalize(this);

            _reader.Dispose();
        }
    }
}
