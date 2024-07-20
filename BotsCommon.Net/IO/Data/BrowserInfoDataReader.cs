using BotsCommon.IO.Data;

namespace BotsCommon.Net.IO.Data
{
    public sealed class BrowserInfoDataReader : IDataReader<BrowserInfo>
    {
        private readonly IDataReader<string> _reader;

        public BrowserInfoDataReader(IDataReader<string> reader)
        {
            _reader = reader;
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public BrowserInfo? Read()
        {
            var str = _reader.Read();

            if (str == null)
                return null;

            return BrowserInfo.Parse(str);
        }

        object? IDataReader.Read()
        {
            return Read();
        }

        public void Reset()
        {
            _reader.Reset();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
