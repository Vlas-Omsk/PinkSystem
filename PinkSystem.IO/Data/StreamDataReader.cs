using System;
using System.IO;

namespace PinkSystem.IO.Data
{
    public abstract class StreamDataReader<T> : IDataReader<T>
    {
        private readonly TextReader _reader;
        private readonly object _lock = new();
        private int _index;

        public StreamDataReader(TextReader reader)
        {
            _reader = reader;

            if (reader is StreamReader streamReader && streamReader.BaseStream.CanSeek)
                Length = streamReader.CountLines();
        }

        public int? Length { get; private set; }
        public int Index
        {
            get
            {
                lock (_lock)
                    return _index;
            }
        }

        public T? Read()
        {
            string? line;

            lock (_lock)
            {
                line = _reader.ReadLine();

                if (line != null)
                {
                    _index++;

                    line = line.TrimStart(new char[] {
                        '\uFEFF',
                        '\u200B'
                    });
                }
            }

            return ReadOverride(line);
        }

        protected abstract T? ReadOverride(string? line);

        public void Reset()
        {
            lock (_lock)
            {
                if (_reader is StreamReader streamReader)
                    streamReader.SetPosition(0);
                else
                    throw new NotSupportedException();

                _index = 0;
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
